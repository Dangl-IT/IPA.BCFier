using IPA.Bcfier.Models.Clashes;

namespace IPA.Bcfier.Navisworks.Models
{
    public class CreateClashIssuesQueueItem
    {
        public Func<string, Task>? Callback { get; set; }

        public NavisworksClashCreationData ClashCreationData { get; set; }
    }
}
