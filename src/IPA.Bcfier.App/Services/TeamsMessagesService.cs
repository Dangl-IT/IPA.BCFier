using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Models.Controllers.TeamsMessages;
using IPA.Bcfier.App.Models.Services.TeamsMessages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Drawing.Text;
using System.Text;

namespace IPA.Bcfier.App.Services
{
    public class TeamsMessagesService
    {
        private readonly BcfierDbContext _context;
        private readonly HttpClient _httpClient;

        public TeamsMessagesService(BcfierDbContext context,
            HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task AnnounceNewCommentInProjectTopicAsync(Guid projectId,
            TeamsMessagePost message)
        {
            var dbProject = await _context
                .Projects
                .Where(p => p.Id == projectId)
                .Select(p => new
                {
                    p.TeamsWebhook,
                    p.Name
                })
                .FirstOrDefaultAsync();
            if (dbProject == default || string.IsNullOrWhiteSpace(dbProject.TeamsWebhook))
            {
                // Not doing anything for projects that either don't exist
                // or don't have a webhook configured
                return;
            }

            var teamsMessage = new TeamsMessage();
            var title = !string.IsNullOrWhiteSpace(message.TopicTitle)
                ? "New topic: " + message.TopicTitle
                : (!string.IsNullOrWhiteSpace(message.Comment)
                    ? "New comment: " + message.Comment
                    : "New viewpoint");

            var mainSection = new TeamsSection
            {
                ActivityTitle = title,
                ActivitySubtitle = "Project: " + dbProject.Name,
                Facts = new List<TeamsFact>
                {
                    new TeamsFact
                    {
                        Name = "Author",
                        Value = message.Username
                    }
                }
            };
            teamsMessage.Sections = new List<TeamsSection> { mainSection };

            teamsMessage.Summary = "Update via BCFier";

            if (!string.IsNullOrWhiteSpace(message.ViewpointBase64))
            {
                teamsMessage.Sections.Add(new TeamsSection
                {
                    Images = new List<TeamsImage>
                    {
                        new TeamsImage
                        {
                            ImageBase64DataUrl = "data:image/png;base64," + message.ViewpointBase64
                        }
                    }
                });
            }

            await SendTeamsMessageAsync(teamsMessage, dbProject.TeamsWebhook);
        }

        private async Task SendTeamsMessageAsync(TeamsMessage message,
            string teamsWebhookUrl)
        {
            // Apparently, the max message size for Teams webhooks is 28.000 characters
            // so we're ensuring we resize images to fit in there
            // Also, we seemed to have problems with a max size of 28.000, so we've lowered it
            // to 25.000
            var messageJson = GetTeamsMessageWithMaxSizeInBytes(message, 25_000);
            if (string.IsNullOrWhiteSpace(messageJson))
            {
                // Looks like the message was too large for the webhook
                return;
            }

            var body = new StringContent(messageJson, Encoding.Default, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, teamsWebhookUrl);
            request.Content = body;

            await _httpClient.SendAsync(request);
        }

        private static string? GetTeamsMessageWithMaxSizeInBytes(TeamsMessage message,
            int maxMessageLength)
        {
            var jsonOptions = new JsonSerializerSettings();
            jsonOptions.Formatting = Formatting.None;
            jsonOptions.NullValueHandling = NullValueHandling.Ignore;
            var messageJson = JsonConvert.SerializeObject(message, jsonOptions);

            if (messageJson.Length <= maxMessageLength)
            {
                return messageJson;
            }

            var maxTries = 5;
            var currentTry = 0;
            while (currentTry < maxTries)
            {
                maxTries++;
                // Let's compress the images
                foreach (var section in message.Sections.Where(s => s.Images != null))
                {
                    foreach (var imageSection in section.Images!.Where(image => !string.IsNullOrWhiteSpace(image.ImageBase64DataUrl)))
                    {
                        var imageBase64 = imageSection.ImageBase64DataUrl.Substring(imageSection.ImageBase64DataUrl.IndexOf(",") + 1);
                        var imageBytes = Convert.FromBase64String(imageBase64);

                        using var image = SixLabors.ImageSharp.Image.Load(new MemoryStream(imageBytes));
                        var width = image.Width / 2;
                        var height = image.Height / 2;
                        image.Mutate(x => x.Resize(width, height));
                        using var outMemStream = new MemoryStream();
                        image.Save(outMemStream, new PngEncoder());
                        imageSection.ImageBase64DataUrl = "data:image/png;base64," + Convert.ToBase64String(outMemStream.ToArray());
                    }
                }

                messageJson = JsonConvert.SerializeObject(message, jsonOptions);

                if (messageJson.Length <= maxMessageLength)
                {
                    return messageJson;
                }
            }

            // If we're still to big, there's nothing we can do - we'll
            // not keep decreasing, since we already halved it 5 times, so it
            // probably was way too big anyway for anything practical here

            return null;
        }
    }
}
