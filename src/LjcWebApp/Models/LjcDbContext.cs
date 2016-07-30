using LjcWebApp.Helper;
using Microsoft.EntityFrameworkCore;

namespace LjcWebApp
{
    public class LjcDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(DbHelper.DbConnectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<configmemorycurve_tb>();
            modelBuilder.Entity<introspect>();
            modelBuilder.Entity<question>();
            modelBuilder.Entity<timestatistic>();
            modelBuilder.Entity<word_tb>();
        }

        public virtual DbSet<configmemorycurve_tb> configmemorycurve_tb { get; set; }
        public virtual DbSet<introspect> introspect { get; set; }
        public virtual DbSet<question> question { get; set; }
        public virtual DbSet<timestatistic> timestatistic { get; set; }
        public virtual DbSet<word_tb> word_tb { get; set; }
    }
}
