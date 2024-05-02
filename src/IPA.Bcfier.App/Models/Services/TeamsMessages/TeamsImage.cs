using Newtonsoft.Json;

namespace IPA.Bcfier.App.Models.Services.TeamsMessages
{
    public class TeamsImage
    {
        [JsonProperty("image")]
        public string ImageBase64DataUrl { get; set; } = string.Empty;
    }
}
