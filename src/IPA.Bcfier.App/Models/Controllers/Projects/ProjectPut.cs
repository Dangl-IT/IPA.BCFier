using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.Projects
{
    public class ProjectPut
    {
        [Required]
        public Guid Id { get; set; } // TODO: do we need this property?

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;

        public string BcfFilesFolder { get; set; } = string.Empty;

        public string RevitFilePath { get; set; } = string.Empty;

        public string? RevitIdentifier { get; set; }

        public string? TeamsWebhook { get; set; }
    }
}
