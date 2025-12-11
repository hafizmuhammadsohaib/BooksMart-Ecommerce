using BooksMart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BooksMart.Data.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<ApplicationUser> applicationUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1, },
                new Category { Id = 2, Name = "Adventure", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Horror", DisplayOrder = 3 }
                );

            modelBuilder.Entity<Company>().HasData(
               new Company
               {
                   Id = 1,
                   Name = "Elare Official",
                   Address = "427 C Block Pak Arab",
                   City = "Lahore",
                   Province = "Punjab",
                   PostalCode = "54600",
                   PhoneNumber = "123-456-7890"
               },
               new Company
               {
                   Id = 2,
                   Name = "ABC Logistics",
                   Address = "Iqbal Town",
                   City = "Lahore",
                   Province = "Punjab",
                   PostalCode = "123456",
                   PhoneNumber = "090078601"
               });

            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Echoes of the Forgotten",
                    AuthorName = "Marvin Cole",
                    Description = "A gripping tale of mystery and redemption set in a forgotten town where secrets slowly unravel. Follow the journey of a man trying to reclaim his past while confronting the shadows that haunt him.",
                    ISBN = "BK10001001",
                    ListPrice = 110,
                    Price = 95,
                    Price50 = 90,
                    Price100 = 85,
                    CategoryId = 1,
                    ImageUrl = ""
                },
                new Book
                {
                    Id = 2,
                    Title = "Whispers of the Valley",
                    AuthorName = "Diana Crest",
                    Description = "A heartwarming story about a young woman's return to her childhood valley, discovering love, loss, and the beauty hidden in everyday life. A gentle reminder that home is more than a place.",
                    ISBN = "BK20002001",
                    ListPrice = 45,
                    Price = 38,
                    Price50 = 33,
                    Price100 = 28,
                    CategoryId = 2,
                    ImageUrl = ""
                },
                new Book
                {
                    Id = 3,
                    Title = "Shadows Over Dawn",
                    AuthorName = "Felix Hart",
                    Description = "A suspense novel that follows an investigator racing against time to uncover a conspiracy threatening the peace of a bustling city. Every clue unravels a deeper layer of danger.",
                    ISBN = "BK30003001",
                    ListPrice = 60,
                    Price = 52,
                    Price50 = 45,
                    Price100 = 40,
                    CategoryId = 3,
                    ImageUrl = ""
                },
                new Book
                {
                    Id = 4,
                    Title = "The Sugar Moon",
                    AuthorName = "Clara Monroe",
                    Description = "An enchanting romantic drama set under the glow of moonlit nights, where two unlikely people cross paths and discover the bittersweet nature of love and destiny.",
                    ISBN = "BK40004001",
                    ListPrice = 75,
                    Price = 70,
                    Price50 = 65,
                    Price100 = 60,
                    CategoryId = 3,
                    ImageUrl = ""
                },
                new Book
                {
                    Id = 5,
                    Title = "Waves of Midnight",
                    AuthorName = "Ernest Hale",
                    Description = "A thrilling ocean adventure following a marine explorer who uncovers a hidden world beneath the waves. Courage, mystery, and danger collide deep in the unknown.",
                    ISBN = "BK50005001",
                    ListPrice = 35,
                    Price = 30,
                    Price50 = 27,
                    Price100 = 22,
                    CategoryId = 1,
                    ImageUrl = ""
                },
                new Book
                {
                    Id = 6,
                    Title = "Garden of Starlight",
                    AuthorName = "Amelia Thorn",
                    Description = "A beautifully written fantasy novel about a young girl's discovery of a magical realm blooming beneath an ancient garden. Wonder, imagination, and bravery shape her unforgettable journey.",
                    ISBN = "BK60006001",
                    ListPrice = 28,
                    Price = 25,
                    Price50 = 23,
                    Price100 = 21,
                    CategoryId = 3,
                    ImageUrl = ""
                });
        }

    }
}
