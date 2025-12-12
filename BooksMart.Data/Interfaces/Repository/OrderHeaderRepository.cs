using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;

namespace BooksMart.Data.Interfaces.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>,IOrderHeaderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrderHeaderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
        public void Update(OrderHeader orderHeader)
        {
            dbContext.OrderHeaders.Update(orderHeader);
        }

        public void UpdateOrderStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb =  dbContext.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null) 
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty( paymentStatus)) 
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionID, string paymentIntentId)
        {
            var orderFromDb = dbContext.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if(!string.IsNullOrEmpty(sessionID)) 
            {
                orderFromDb.SessionId = sessionID;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
