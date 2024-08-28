using System;

namespace IPA.Bcfier.Models.Clashes
{
    public class NavisworksClashSelection
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public bool IsGroup { get; set; }
    }
}
