using System;

namespace IPA.Bcfier.Models.Clashes
{
    public class NavisworksClashGroupingData
    {
        public Guid ClashId { get; set; }

        public GroupingType GroupingType { get; set; }

        public ProximityGroupingOptions? ProximityGroupingOptions { get; set; }

        public LevelGroupingOptions? LevelGroupingOptions { get; set; }
    }
}
