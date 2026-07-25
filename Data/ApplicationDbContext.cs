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
    }
}