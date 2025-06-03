using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using IPA.Bcfier.Models.Clashes;
using IPA.Bcfier.Navisworks.Models;

namespace IPA.Bcfier.Navisworks.Services
{
    public static class NavisworksClashGroupingService
    {
        public static List<Guid> GroupClashes(NavisworksClashGroupingData clashGroupingData)
        {
            switch (clashGroupingData.GroupingType)
            {
                case GroupingType.Proximity:
                    return GroupClashesByProximity(clashGroupingData);
                case GroupingType.Level:
                    return GroupClashesByLevel(clashGroupingData);
                case GroupingType.Selection:
                    return GroupClashesBySelection(clashGroupingData);
                default:
                    throw new NotImplementedException(); // TODO
            }
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
            /*
            var clashTest = Application.MainDocument.GetClash().TestsData.Tests.OfType<ClashTest>().FirstOrDefault(t => t.Guid == clashGroupingData.ClashTestId);
            if (clashTest == null)
            {
                throw new NotImplementedException(); // TODO
            }

            var result = new List<Guid>();
            foreach (var clash in clashTest.Children.OfType<ClashResult>())
            {
                if (ContainsElementId(clash.Item1, clashGroupingData.SelectionGroupingOptions!.ElementId) ||
                    ContainsElementId(clash.Item2, clashGroupingData.SelectionGroupingOptions!.ElementId))
                {
                    result.Add(clash.Guid);
                }
            }

            return result;
            */
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
