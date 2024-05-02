using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IPA.Bcfier.App.Data.Models
{
    public class LastOpenedUserFile
    {
        public Guid Id { get; set; }

        public Guid? ProjectId { get; set; }

        public Project? Project { get; set; }

        public DateTimeOffset OpenedAtAtUtc { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LastOpenedUserFile>()
                .HasIndex(x => x.UserName);
        }
    }
}
