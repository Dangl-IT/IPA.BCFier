using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.ProjectUsers
{
    public class ProjectUserPost
    {
        [Required]
        public string Identifier { get; set; } = string.Empty;
    }
}
