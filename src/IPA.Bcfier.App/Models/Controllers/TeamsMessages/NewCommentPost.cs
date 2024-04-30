using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.TeamsMessages
{
    public class TeamsMessagePost
    {
        public string? TopicTitle { get; set; }

        public string? Comment { get; set; }

        public string? ViewpointBase64 { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
