using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.Projects
{
    public class ProjectGet
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? RevitIdentifier { get; set; }

        public string? TeamsWebhook { get; set; }
    }
}
