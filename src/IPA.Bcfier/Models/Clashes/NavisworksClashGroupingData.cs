using System;

namespace IPA.Bcfier.Models.Clashes
{
    public class NavisworksClashGroupingData
    {
        public Guid ClashTestId { get; set; }

        public GroupingType GroupingType { get; set; }

        public ProximityGroupingOptions? ProximityGroupingOptions { get; set; }

        public LevelGroupingOptions? LevelGroupingOptions { get; set; }

        public SelectionGroupingOptions? SelectionGroupingOptions { get; set; }
    }
}
