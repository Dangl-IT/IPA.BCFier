using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Interop;
using Autodesk.Navisworks.Internal.ApiImplementation;
using IPA.Bcfier.Models.Clashes;
using IPA.Bcfier.Navisworks.Models;
using System.Diagnostics;

namespace IPA.Bcfier.Navisworks.Services
{
    public static class NavisworksClashGroupingService
    {
        private static List<PreviousClashState> _previousClashState = new List<PreviousClashState>();

        public static List<Guid> GroupClashes(NavisworksClashGroupingData clashGroupingData)
        {
            List<Guid>? clashIds = null;
            switch (clashGroupingData.GroupingType)
            {
                case GroupingType.Proximity:
                    clashIds = GroupClashesByProximity(clashGroupingData);
                    break;
                case GroupingType.Level:
                    clashIds = GroupClashesByLevel(clashGroupingData);
                    break;
                case GroupingType.Selection:
                    clashIds =  GroupClashesBySelection(clashGroupingData);
                    break;
                default:
                    throw new NotImplementedException();
            }

            GroupClashesInClashDetective(clashIds);
            return clashIds;
        }

        private static List<Guid> GroupClashesByProximity(NavisworksClashGroupingData clashGroupingData)
        {
            return GroupClashesByGeometryCheck(clashGroupingData, (baseCenter, otherCenter) =>
                {
                    var actualDistance = Math.Abs(otherCenter.DistanceTo(baseCenter));
                    return actualDistance <= clashGroupingData.ProximityGroupingOptions!.Radius;
                });
        }

        private static List<Guid> GroupClashesByLevel(NavisworksClashGroupingData clashGroupingData)
        {
            return GroupClashesByGeometryCheck(clashGroupingData, (baseCenter, otherCenter) =>
            {
                var zLevelDistance = Math.Abs(otherCenter.Z - baseCenter.Z);
                return zLevelDistance <= clashGroupingData.LevelGroupingOptions!.Tolerance;
            });
        }

        private static List<Guid> GroupClashesByGeometryCheck(NavisworksClashGroupingData clashGroupingData, Func<Point3D, Point3D, bool> checkFunction)
        {
            var doc = Application.MainDocument;
            var tests = doc.GetClash().TestsData.Tests;

            var testItems = tests
                .OfType<ClashTest>()
                .Where(t => t.Children.Count > 0)
                .SelectMany(t => t.Children.Select(tt => new ClashTestWrapper
                {
                    TestDisplayName = t.DisplayName,
                    SavedItem = tt,
                    ClashTest = t
                }))
                .ToList();

            var clashResults = new List<Guid> { clashGroupingData.ClashId };

            Point3D baseCenter = null;
            foreach (var testItem in testItems)
            {
                if (testItem.SavedItem is ClashResult result && result.Guid == clashGroupingData.ClashId)
                {
                    baseCenter = result.Center;
                    break;
                }
                else if (testItem.SavedItem is ClashResultGroup resultGroup && resultGroup.Guid == clashGroupingData.ClashId)
                {
                    baseCenter = resultGroup.Center;
                    break;
                }
            }

            if (baseCenter == null)
            {
                return clashResults;
            }

            foreach (var testItem in testItems)
            {
                if (testItem.SavedItem is ClashResult result)
                {
                    var isPassingCheck = checkFunction(baseCenter, result.Center);
                    if (isPassingCheck)
                    {
                        clashResults.Add(result.Guid);
                    }

                }
                else if (testItem.SavedItem is ClashResultGroup resultGroup)
                {
                    var isPassingCheck = checkFunction(baseCenter, resultGroup.Center);
                    if (isPassingCheck)
                    {
                        clashResults.Add(resultGroup.Guid);
                    }
                }
            }

            return clashResults;
        }

        private static List<Guid> GroupClashesBySelection(NavisworksClashGroupingData clashGroupingData)
        {
            // TODO, we're currently not using this, it's all done in the frontend
            throw new System.NotImplementedException();
        }

        public static bool GroupClashesInClashDetective(List<Guid> clashIds)
        {
            var groupingDisplayName = "IPA.BCFier Group";
            var doc = Application.MainDocument;
            var testData = doc.GetClash().TestsData;
            var tests = testData.Tests;
            var clashTestGroup = testData.Tests
                .OfType<ClashTest>()
                .FirstOrDefault(t => t.DisplayName == groupingDisplayName
                || t.CustomTestName == groupingDisplayName);

            var hasDuplicates = clashIds.Count != clashIds.Distinct().Count();

            if (clashTestGroup == null)
            {
                return false;
            }

            var testItems = tests
                .OfType<ClashTest>()
                .Where(t => t.Children.Count > 0)
                .SelectMany(t => t.Children.Select(tt => new ClashTestWrapper
                {
                    TestDisplayName = t.DisplayName,
                    SavedItem = tt,
                    ClashTest = t
                }))
                .ToList();

            // First, we're restoring the original state if there was one
            foreach (var entry in _previousClashState)
            {
                var testEntry = testItems
                    .Single(ti => ti.SavedItem?.Guid == entry.ElementId);
                var oldParent = testData.Tests
                    .OfType<ClashTest>()
                    .Single(t => t.Guid == entry.OldParentId);

                if (testEntry.SavedItem is ClashResult result)
                {
                    testData.TestsMove(result.Parent,
                        result.Parent.Children.IndexOf(result),
                        oldParent,
                        entry.OldParentIndex);

                }
                else if (testEntry.SavedItem is ClashResultGroup resultGroup)
                {
                    testData.TestsMove(resultGroup.Parent,
                        resultGroup.Parent.Children.IndexOf(resultGroup),
                        oldParent,
                        entry.OldParentIndex);
                }
            }
            _previousClashState.Clear();

            // Then, we'll actually move the clashes to the group
            var newIndex = 0;
            foreach (var testItem in testItems
                .Select(ti => ti.SavedItem)
                .Where(si => clashIds.Contains(si.Guid)))
            {
                if (testItem is ClashResult result)
                {
                    if (result.Parent.Guid == clashTestGroup?.Guid
                        || result.Parent.Guid == clashTestGroup?.Parent?.Guid)
                    {
                        continue;
                    }

                    if (result.Parent == clashTestGroup || IsDescendantOrSelf(clashTestGroup, result))
                    {
                        continue;
                    }

                    var oldIndex = result.Parent.Children.IndexOf(result);
                    _previousClashState.Add(new PreviousClashState
                    {
                        ElementId = result.Guid,
                        OldParentId = result.Parent.Guid,
                        OldParentIndex = oldIndex
                    });

                    MoveTest(result.Parent,
                        oldIndex,
                        clashTestGroup,
                        newIndex++);
                }
                else if (testItem is ClashResultGroup resultGroup)
                {
                    if (resultGroup.Parent.Guid == clashTestGroup?.Guid
                        || resultGroup.Parent.Guid == clashTestGroup?.Parent?.Guid)
                    {
                        continue;
                    }

                    if (resultGroup.Parent == clashTestGroup || IsDescendantOrSelf(clashTestGroup, resultGroup))
                    {
                        continue;
                    }

                    var oldIndex = resultGroup.Parent.Children.IndexOf(resultGroup);
                    _previousClashState.Add(new PreviousClashState
                    {
                        ElementId = resultGroup.Guid,
                        OldParentId = resultGroup.Parent.Guid,
                        OldParentIndex = oldIndex
                    });

                    MoveTest(resultGroup.Parent,
                        oldIndex,
                        clashTestGroup,
                        newIndex++);
                }
            }

            return true;
        }

        /// <summary>
        /// This is basically the implementation from the official Navisworks API, but pasted here since we were getting
        /// StackOverflowExceptions when we called the original API.
        /// </summary>
        /// <param name="oldParent"></param>
        /// <param name="oldIndex"></param>
        /// <param name="newParent"></param>
        /// <param name="newIndex"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void MoveTest(GroupItem oldParent, int oldIndex, GroupItem newParent, int newIndex)
        {
            var m_document = Application.MainDocument;
            var Value = m_document.GetClash().TestsData.Value;

            if ((object)Autodesk.Navisworks.Api.Interop.LcOpClashElement.Get(m_document.State) == null)
            {
                throw new ArgumentOutOfRangeException("oldIndex");
            }

            GroupItem groupItem = (((object)oldParent == null) ? Value.TestsRoot : oldParent);
            GroupItem groupItem2 = (((object)newParent == null) ? Value.TestsRoot : newParent);
            if (Value.TestsRoot != groupItem)
            {
                if (!DeepContains(Value.TestsRoot, groupItem))
                {
                    throw new ArgumentException(DocumentClashTestsExceptions.NotInClashMessage, "oldParent");
                }
            }

            if (Value.TestsRoot != groupItem2)
            {
                if (!DeepContains(Value.TestsRoot, groupItem2))
                {
                    throw new ArgumentException(DocumentClashTestsExceptions.NotInClashMessage, "newParent");
                }
            }

            CollectionImpl<SavedItem>.ValidateIndex(groupItem.Children, oldIndex);
            if (groupItem == groupItem2 && newIndex > oldIndex)
            {
                CollectionImpl<SavedItem>.ValidateIndex(groupItem2.Children, newIndex);
                newIndex++;
            }
            else
            {
                CollectionImpl<SavedItem>.ValidateInsertIndex(groupItem2.Children, newIndex);
            }

            SavedItem item = groupItem.Children[oldIndex];
            string paramName = "moving item";
            if (!groupItem2.CanAddType(item))
            {
                throw new ArgumentException(DocumentClashTestsExceptions.UnsupportedSavedItemMessage, paramName);
            }

            bool condition = Autodesk.Navisworks.Api.Interop.LcOpClashElement.MoveTest(m_document.State, item, newParent, newIndex);
            Debug.Assert(condition);
        }

        private static bool DeepContains(GroupItem group, SavedItem item)
        {
            if (group.Children.Contains(item))
            {
                return true;
            }

            foreach (SavedItem child in group.Children)
            {
                if (child is GroupItem group2 && DeepContains(group2, item))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDescendantOrSelf(ClashTest possibleParent, SavedItem item)
        {
            if (item is ClashResult cr)
            {
                var parent = cr.Parent;
                while (parent != null)
                {
                    if (parent == possibleParent)
                        return true;
                    parent = parent.Parent;
                }
            }
            else if (item is ClashResultGroup crg)
            {
                var parent = crg.Parent;
                while (parent != null)
                {
                    if (parent == possibleParent)
                        return true;
                    parent = parent.Parent;
                }
            }
            return false;
        }

        private class ClashInfo
        {
            public Guid ClashId { get; set; }
            public Point3D? Position { get; set; }
        }

        private class UnionFind
        {
            private readonly Dictionary<Guid, Guid> parent = new();

            public Guid Find(Guid x)
            {
                if (!parent.ContainsKey(x))
                {
                    parent[x] = x;
                }

                if (!parent[x].Equals(x))
                {
                    parent[x] = Find(parent[x]);
                }

                return parent[x];
            }

            public void Union(Guid x, Guid y)
            {
                var rootX = Find(x);
                var rootY = Find(y);
                if (!rootX.Equals(rootY))
                {
                    parent[rootY] = rootX;
                }
            }
        }

        private static double Distance(Point3D a, Point3D b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static bool ContainsElementId(ModelItem item, string elementId)
        {
            foreach (var descendant in item.DescendantsAndSelf)
            {
                foreach (var propCategory in descendant.PropertyCategories)
                {
                    foreach (var prop in propCategory.Properties)
                    {
                        if (prop.DisplayName.Equals("Element ID", StringComparison.OrdinalIgnoreCase) ||
                            prop.DisplayName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            if (prop.Value.ToDisplayString().Equals(elementId, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
