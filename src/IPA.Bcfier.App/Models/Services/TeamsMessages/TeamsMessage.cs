using Newtonsoft.Json;

namespace IPA.Bcfier.App.Models.Services.TeamsMessages
{
    public class TeamsMessage
    {
        [JsonProperty("type")]
        public string Type { get; } = "message";

        [JsonProperty("attachments")]
        public List<TeamsAttachment> Attachments { get; set; } = new List<TeamsAttachment>();
    }
}
