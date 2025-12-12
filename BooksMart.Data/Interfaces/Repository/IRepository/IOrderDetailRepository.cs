using BooksMart.Models.Models;
namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail orderDetail);
    }
}
