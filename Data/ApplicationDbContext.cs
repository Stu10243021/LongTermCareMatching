using Microsoft.EntityFrameworkCore;
using LongTermCareMatching.Models;

namespace LongTermCareMatching.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseComment> CaseComments { get; set; }
        public DbSet<CaseApplication> CaseApplications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}