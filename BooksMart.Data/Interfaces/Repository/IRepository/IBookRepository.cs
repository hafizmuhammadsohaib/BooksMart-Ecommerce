using BooksMart.Models.Models;

namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IBookRepository : IRepository<Book>
    {
        void Update(Book book);
    }
}
