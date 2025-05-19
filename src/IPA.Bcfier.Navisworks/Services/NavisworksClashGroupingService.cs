using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using IPA.Bcfier.Models.Clashes;

namespace IPA.Bcfier.Navisworks.Services
{
    public static class NavisworksClashGroupingService
    {
        public static List<List<Guid>> GroupClashes(NavisworksClashGroupingData clashGroupingData)
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

        private static List<List<Guid>> GroupClashesByProximity(NavisworksClashGroupingData clashGroupingData)
        {
            var clashTest = Application.MainDocument.GetClash().TestsData.Tests.OfType<ClashTest>().FirstOrDefault(t => t.Guid == clashGroupingData.ClashTestId);
            if (clashTest == null)
            {
                throw new NotImplementedException(); // TODO
            }

            var clashInfos = clashTest.Children.OfType<ClashResult>().Where(x => clashGroupingData.ProximityGroupingOptions!.ClashIds.Contains(x.Guid))
                .Select(x => new ClashInfo
                {
                    ClashId = x.Guid,
                    Position = x.Center
                }).ToList();

            var uf = new UnionFind();
            for (int i = 0; i < clashInfos.Count; i++)
            {
                for (int j = i + 1; j < clashInfos.Count; j++)
                {
                    if (Distance(clashInfos[i].Position!, clashInfos[j].Position!) <= (2 * clashGroupingData.ProximityGroupingOptions!.Radius))
                    {
                        uf.Union(clashInfos[i].ClashId, clashInfos[j].ClashId);
                    }
                }
            }

            var groups = new Dictionary<Guid, List<Guid>>();
            foreach (var clash in clashInfos)
            {
                var root = uf.Find(clash.ClashId);
                if (!groups.ContainsKey(root))
                {
                    groups[root] = new List<Guid>();
                }

                groups[root].Add(clash.ClashId);
            }

            return groups.Values.ToList();
        }

        private static List<List<Guid>> GroupClashesByLevel(NavisworksClashGroupingData clashGroupingData)
        {
            throw new NotImplementedException();
        }

        private static List<List<Guid>> GroupClashesBySelection(NavisworksClashGroupingData clashGroupingData)
        {
            throw new NotImplementedException();
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
    }
}
