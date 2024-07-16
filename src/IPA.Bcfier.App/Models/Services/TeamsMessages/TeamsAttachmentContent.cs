using Newtonsoft.Json;

namespace IPA.Bcfier.App.Models.Services.TeamsMessages
{
    public class TeamsAttachmentContent
    {
        [JsonProperty("@schema")]
        public string Schema { get; } = "http://adaptivecards.io/schemas/adaptive-card.json";

        [JsonProperty("type")]
        public string Type { get; } = "AdaptiveCard";

        [JsonProperty("version")]
        public string Version { get; } = "1.2";

        [JsonProperty("body")]
        public List<TeamsAttachmentContentBody> Body { get; set; } = new List<TeamsAttachmentContentBody>();
    }
}
