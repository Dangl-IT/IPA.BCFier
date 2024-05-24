using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.Models.Config
{
    public class FrontendConfig
    {
        [Required]
        public bool IsInElectronMode { get; set; } = false;

        [Required]
        public bool IsConnectedToRevit { get; set; } = false;

        [Required]
        public bool IsConnectedToNavisworks { get; set; } = false;

        public string? RevitProjectPath { get; set; }

        [Required]
        public string Environment { get; set; } = string.Empty;
    }
}
