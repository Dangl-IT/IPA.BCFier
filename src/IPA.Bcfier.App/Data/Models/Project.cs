namespace IPA.Bcfier.App.Data.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string RevitIdentifer { get; set; } = string.Empty;

        public string? TeamsWebhook { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}
