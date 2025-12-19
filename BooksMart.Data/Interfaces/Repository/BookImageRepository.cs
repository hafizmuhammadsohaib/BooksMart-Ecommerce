using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;

namespace BooksMart.Data.Interfaces.Repository
{
    public class BookImageRepository : Repository<BookImage>,IBookImageRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BookImageRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
        public void Update(BookImage bookImage)
        {
            dbContext.BookImages.Update(bookImage);
        }
    }
}
