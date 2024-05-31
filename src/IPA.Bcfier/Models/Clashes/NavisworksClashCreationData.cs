using System;
using System.Collections.Generic;

namespace IPA.Bcfier.Models.Clashes
{
    public class NavisworksClashCreationData
    {
        public Guid ClashId { get; set; }

        public List<Guid> ExcludedClashIds { get; set; } = new List<Guid>();
    }
}
