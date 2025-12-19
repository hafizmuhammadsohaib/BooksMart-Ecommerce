using BooksMart.Models.Models;
namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IBookImageRepository : IRepository<BookImage>
    {
        void Update(BookImage bookImage);
    }
}
