using Newtonsoft.Json;

namespace IPA.Bcfier.App.Models.Services.TeamsMessages
{
    public class TeamsAttachment
    {
        [JsonProperty("contentType")]
        public string ContentType { get; } = "application/vnd.microsoft.card.adaptive";

        [JsonProperty("content")]
        public TeamsAttachmentContent Content { get; set; } = new TeamsAttachmentContent();
    }
}
