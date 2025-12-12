using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;

namespace BooksMart.Data.Interfaces.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;
        public ICategoryRepository Category { get; private set; }

        public IBookRepository Book { get; private set; }
        public ICompanyRepository Company { get; private set; } 
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            Category = new CategoryRepository(dbContext);
            Book = new BookRepository(dbContext);
            Company = new CompanyRepository(dbContext);
            ShoppingCart = new ShoppingCartRepository(dbContext);
            ApplicationUser = new ApplicationUserRepository(dbContext);
            OrderHeader = new OrderHeaderRepository(dbContext);
            OrderDetail = new OrderDetailRepository(dbContext);
        }

        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
