using DragonBallApi.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace DragonBallApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Character> Characters { get; set; }
        public DbSet<Transformation> Transformations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Character>()
                .HasMany(c => c.Transformations)       
                .WithOne(t => t.Character)             
                .HasForeignKey(t => t.CharacterId)     
                .IsRequired();
            modelBuilder.Entity<Character>()
                .HasIndex(c => c.Name);
             modelBuilder.Entity<Character>()
                .HasIndex(c => c.Affiliation);
                
        }
    }
}