using System.Linq.Expressions;

namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task< IEnumerable<T>> GetAll(Expression<Func<T, bool>>? expression = null,string? includeProperties = null);
        Task<T?> GetByIdAsync(Expression<Func<T, bool>> expression, string? includeProperties = null, bool tracked = false);
        Task AddAsync(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task<int> CountAsync(Expression<Func<T, bool>>? expression = null);
    }
}
