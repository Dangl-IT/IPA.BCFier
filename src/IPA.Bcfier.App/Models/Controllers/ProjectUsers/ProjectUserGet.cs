using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.ProjectUsers
{
    public class ProjectUserGet
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Identifier { get; set; } = string.Empty;
    }
}
