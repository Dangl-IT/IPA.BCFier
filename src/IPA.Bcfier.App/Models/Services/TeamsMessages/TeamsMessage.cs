using Newtonsoft.Json;

namespace IPA.Bcfier.App.Models.Services.TeamsMessages
{
    public class TeamsMessage
    {
        [JsonProperty("@type")]
        public string Type { get; } = "MessageCard";

        [JsonProperty("@context")]
        public string Context { get; } = "http://schema.org/extensions";

        [JsonProperty("themeColor")]
        public string ThemeColor { get; } = "0076D7";
        //public string ThemeColor { get; } = "eed300";

        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonProperty("sections")]
        public List<TeamsSection> Sections { get; set; } = new List<TeamsSection>();
    }
}
