using BooksMart.Models.Models;
namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
    }
}
