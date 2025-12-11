using BooksMart.Models.Models;
namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);
    }
}
