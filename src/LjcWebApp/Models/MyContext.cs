using Microsoft.EntityFrameworkCore;

namespace LjcWebApp.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<configmemorycurve_tb>();
            modelBuilder.Entity<introspect>();
            modelBuilder.Entity<question>();
            modelBuilder.Entity<timestatistic>();
            modelBuilder.Entity<word_tb>();
        }

        public virtual DbSet<configmemorycurve_tb> Configmemorycurve { get; set; }
        public virtual DbSet<introspect> Introspect { get; set; }
        public virtual DbSet<question> Question { get; set; }
        public virtual DbSet<timestatistic> Timestatistic { get; set; }
        public virtual DbSet<word_tb> Word { get; set; }
    }
}
