using Newtonsoft.Json;

namespace IPA.Bcfier.App.Models.Services.TeamsMessages
{
    public class TeamsSection
    {
        [JsonProperty("activityTitle")]
        public string? ActivityTitle { get; set; }

        [JsonProperty("activitySubtitle")]
        public string? ActivitySubtitle { get; set; }

        [JsonProperty("facts")]
        public List<TeamsFact>? Facts { get; set; }

        [JsonProperty("images")]
        public List<TeamsImage>? Images { get; set; }
    }
}
