using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;

namespace BooksMart.Data.Interfaces.Repository
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BookRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
        public void Update(Book book)
        {
            var objFromDb = dbContext.Books.FirstOrDefault(b => b.Id == book.Id);
            if (objFromDb != null) 
            { 
                objFromDb.Title = book.Title;
                objFromDb.Description = book.Description;
                objFromDb.ISBN = book.ISBN;
                objFromDb.AuthorName = book.AuthorName;
                objFromDb.ListPrice = book.ListPrice;
                objFromDb.Price = book.Price;
                objFromDb.Price50= book.Price50;
                objFromDb.Price100 = book.Price100;
                objFromDb.CategoryId = book.CategoryId;
                if (book.ImageUrl != null)
                {
                    objFromDb.ImageUrl = book.ImageUrl;
                }
            }
            //dbContext.Books.Update(book);
        }
    }
}
