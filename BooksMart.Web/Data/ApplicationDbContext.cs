using BooksMart.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksMart.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category{ Id = 1, Name = "Action", DisplayOrder = 1,},
                new Category{ Id = 2, Name="Adventure", DisplayOrder = 2},
                new Category{ Id = 3, Name="Horror", DisplayOrder = 3}
                );
        }
        

    }
}
