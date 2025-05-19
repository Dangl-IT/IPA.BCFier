using System;
using System.Collections.Generic;

namespace IPA.Bcfier.Models.Clashes
{
    public class ProximityGroupingOptions
    {
        public List<Guid> ClashIds { get; set; } = new List<Guid>();

        public double Radius { get; set; }
    }
}
