using IPA.Bcfier.Models.Clashes;

namespace IPA.Bcfier.Navisworks.Models
{
    public class CreateClashIssuesQueueItem
    {
        public Func<string, Task>? Callback { get; set; }

        public Action<int>? CallbackReportTotalCount { get; set; }

        public Action<int>? CallbackReportCurrentCount { get; set; }

        public NavisworksClashCreationData ClashCreationData { get; set; }
    }
}
