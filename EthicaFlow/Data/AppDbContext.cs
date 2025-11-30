using EthicaFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace EthicaFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<EthicsSubmission> EthicsSubmissions { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ReviewDecision> ReviewDecisions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}