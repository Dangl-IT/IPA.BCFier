using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.LastOpenedFiles
{
    public class LastOpenedFileGet
    {
        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset OpenedAtUtc { get; set; }
    }
}
