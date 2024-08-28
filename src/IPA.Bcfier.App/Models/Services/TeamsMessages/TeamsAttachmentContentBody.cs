using Newtonsoft.Json;

namespace IPA.Bcfier.App.Models.Services.TeamsMessages
{
    public class TeamsAttachmentContentBody
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string? Text { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string? Url { get; set; } = string.Empty;
    }
}
