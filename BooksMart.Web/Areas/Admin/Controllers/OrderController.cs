using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using BooksMart.Models.ViewModels;
using BooksMart.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BooksMart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int orderId)
        {
            orderVM = new()
            {
                OrderHeader = await unitOfWork.OrderHeader.GetByIdAsync(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = await unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Book")
            };
            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles =CD.Role_Admin+","+CD.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetails()
        {
            var orderHeaderFromDb = await unitOfWork.OrderHeader.GetByIdAsync(u => u.Id == orderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.Address = orderVM.OrderHeader.Address;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.Province = orderVM.OrderHeader.Province;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.TrackingNumber;
            }
            unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            await unitOfWork.SaveAsync();
            TempData["success"] = "Order details updated successfully.";
            return RedirectToAction(nameof(Details),new {orderId=orderHeaderFromDb.Id});
        }
        [HttpPost]
        [Authorize(Roles = CD.Role_Admin + "," + CD.Role_Employee)]
        public async Task<IActionResult> StartProcessing()
        {
            unitOfWork.OrderHeader.UpdateOrderStatus(orderVM.OrderHeader.Id, CD.StatusInProcess);
            await unitOfWork.SaveAsync();
            TempData["success"] = "Order status updated to In Process.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = CD.Role_Admin + "," + CD.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            var orderHeader = await unitOfWork.OrderHeader.GetByIdAsync(i => i.Id == orderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus= CD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if (orderHeader.PaymentStatus == CD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            unitOfWork.OrderHeader.Update(orderHeader);
            await unitOfWork.SaveAsync();
            TempData["success"] = "Order Shipped Successfully!";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = CD.Role_Admin + "," + CD.Role_Employee)]
        public async Task<IActionResult> CancelOrder()
        {
            var orderHeader = await unitOfWork.OrderHeader.GetByIdAsync(i => i.Id == orderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == CD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                unitOfWork.OrderHeader.UpdateOrderStatus(orderHeader.Id, CD.StatusCancelled, CD.StatusRefunded);
            }
            else
            {
                unitOfWork.OrderHeader.UpdateOrderStatus(orderHeader.Id, CD.StatusCancelled, CD.StatusCancelled);
            }
            await unitOfWork.SaveAsync();
            TempData["success"] = "Order Cancelled Successfully!";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public async Task<IActionResult> Details_PAY_NOW()
        {

            orderVM.OrderHeader = await unitOfWork.OrderHeader.GetByIdAsync(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
                orderVM.OrderDetail = await unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Book");
            var domain = Request.Scheme+ "://"+Request.Host.Value+"/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in orderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Book.Title
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            unitOfWork.OrderHeader.UpdateStripePaymentID(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            await unitOfWork.SaveAsync();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = await unitOfWork.OrderHeader.GetByIdAsync(
                x => x.Id == orderHeaderId);

            if (orderHeader.PaymentStatus == CD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeader.UpdateOrderStatus(orderHeaderId, orderHeader.OrderStatus, CD.PaymentStatusApproved);
                    await unitOfWork.SaveAsync();
                }
            }
            return View(orderHeaderId);
        }


        #region API_CALLS
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(string status)
        {
            IEnumerable<OrderHeader> orderHeader;
            if(User.IsInRole(CD.Role_Admin) || User.IsInRole(CD.Role_Employee))
            {
                orderHeader = await unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else {                 
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeader = await unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }


            switch (status)
            {
                case "pending":
                    orderHeader = orderHeader.Where(o => o.PaymentStatus == CD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeader = orderHeader.Where(o => o.OrderStatus == CD.StatusInProcess);
                    break;
                case "completed":
                    orderHeader = orderHeader.Where(o => o.OrderStatus == CD.StatusShipped);
                    break;
                case "approved":
                    orderHeader = orderHeader.Where(o => o.OrderStatus == CD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = orderHeader });
        }
        #endregion
    }
}
