using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;

namespace BooksMart.Data.Interfaces.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;
        public ICategoryRepository Category { get; private set; }

        public IBookRepository Book { get; private set; }

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            Category = new CategoryRepository(dbContext);
            Book = new BookRepository(dbContext);
        }

        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
