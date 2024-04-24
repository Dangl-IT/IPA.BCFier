using IPA.Bcfier.App.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace IPA.Bcfier.App.Data
{
    public class BcfierDbContext : DbContext
    {
        public BcfierDbContext(DbContextOptions<BcfierDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<ProjectUser> ProjectUsers { get; set; }
    }
}
