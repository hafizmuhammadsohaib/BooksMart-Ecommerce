using System.Linq.Expressions;

namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task< IEnumerable<T>> GetAll(string? includeProperties = null);
        Task<T?> GetByIdAsync(Expression<Func<T, bool>> expression, string? includeProperties = null);
        Task AddAsync(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}
