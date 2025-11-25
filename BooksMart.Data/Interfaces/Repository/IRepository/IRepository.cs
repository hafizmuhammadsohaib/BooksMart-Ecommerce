using System.Linq.Expressions;

namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //Category
        Task <IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Expression<Func<T, bool>> expression);// To Use LINQ (FirstOrDefault)
        Task AddAsync(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}
