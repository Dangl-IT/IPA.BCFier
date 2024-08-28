using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Models.Controllers.LastOpenedFiles
{
    public class LastOpenedFilesWrapperGet
    {
        [Required]
        public List<LastOpenedFileGet> LastOpenedFiles { get; set; } = new List<LastOpenedFileGet>();
    }
}
