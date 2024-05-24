namespace IPA.Bcfier.Navisworks.Models
{
    public class CreateClashIssuesQueueItem
    {
        public Func<string, Task>? Callback { get; set; }

        public Guid ClashId { get; set; }
    }
}
