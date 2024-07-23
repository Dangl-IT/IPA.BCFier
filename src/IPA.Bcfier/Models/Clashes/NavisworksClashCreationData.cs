using System;
using System.Collections.Generic;

namespace IPA.Bcfier.Models.Clashes
{
    public class NavisworksClashCreationData
    {
        public Guid ClashId { get; set; }

        /// <summary>
        /// This is used to filter for only a specific status of clashes
        /// </summary>
        public string? Status { get; set; }

        public List<Guid> ExcludedClashIds { get; set; } = new List<Guid>();
    }
}
