using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api;

namespace IPA.Bcfier.Navisworks.Models
{
    internal class ClashTestWrapper
    {
        public string? TestDisplayName { get; set; }

        public SavedItem? SavedItem { get; set; }

        public ClashTest? ClashTest { get; set; }
    }
}
