

namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        Task SaveAsync();
    }
}
