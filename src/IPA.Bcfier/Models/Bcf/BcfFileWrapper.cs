using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.Models.Bcf
{
    public class BcfFileWrapper
    {
        [Required]
        public string FileName { get; set; } = string.Empty;

        public BcfFile? BcfFile { get; set; }
    }
}
