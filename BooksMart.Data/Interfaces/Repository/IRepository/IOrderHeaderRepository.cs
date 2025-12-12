using BooksMart.Models.Models;
namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateOrderStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id, string sessionID, string paymentIntentId);
    }
}
