namespace IPA.Bcfier.App.Data.Models
{
    public class ProjectUser
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public Project? Project { get; set; }

        public Guid UserId { get; set; }

        public User? User { get; set; }
    }
}
