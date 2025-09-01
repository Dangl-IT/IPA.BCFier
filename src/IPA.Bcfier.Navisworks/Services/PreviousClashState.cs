namespace IPA.Bcfier.Navisworks.Services
{
    internal class PreviousClashState
    {
        public Guid ElementId { get; set; }
        public Guid OldParentId { get; set; }
        public int OldParentIndex { get; set; }
    }
}
