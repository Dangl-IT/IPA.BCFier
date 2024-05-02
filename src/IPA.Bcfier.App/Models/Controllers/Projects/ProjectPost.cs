using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.Projects
{
    public class ProjectPost
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? RevitIdentifier { get; set; }

        public string? TeamsWebhook { get; set; }
    }
}
