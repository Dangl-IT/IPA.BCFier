using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.Users
{
    public class UserGet
    {
        public Guid Id { get; set; }

        [Required]
        public string Identifier { get; set; } = string.Empty;
    }
}
