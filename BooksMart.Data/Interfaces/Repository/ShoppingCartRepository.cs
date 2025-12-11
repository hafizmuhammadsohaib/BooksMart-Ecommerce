using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;

namespace BooksMart.Data.Interfaces.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>,IShoppingCartRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ShoppingCartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
        public void Update(ShoppingCart shoppingCart)
        {
            dbContext.ShoppingCarts.Update(shoppingCart);
        }
    }
}
