using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.ProjectUsers
{
    public class ProjectUserPost
    {
        public Guid? UserId { get; set; }

        public string? Identifier { get; set; }
    }
}
