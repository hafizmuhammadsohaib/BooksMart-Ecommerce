using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using BooksMart.Models.ViewModels;
using BooksMart.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BooksMart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = await unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId,
                includeProperties: "Book"),
                OrderHeader = new()
            };

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = CalculateTotal(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        public async Task<IActionResult> Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = await unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId,
                includeProperties: "Book"),
                OrderHeader = new()
            };

            shoppingCartVM.OrderHeader.ApplicationUser = await unitOfWork.ApplicationUser.GetByIdAsync(
                x => x.Id == userId);

            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.Address = shoppingCartVM.OrderHeader.ApplicationUser.Address;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.Province = shoppingCartVM.OrderHeader.ApplicationUser.Province;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;



            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = CalculateTotal(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM.ShoppingCartList = await unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId,
                includeProperties: "Book");

            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser= await unitOfWork.ApplicationUser.GetByIdAsync(
                x => x.Id == userId);

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = CalculateTotal(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.OrderHeader.PaymentStatus = CD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = CD.StatusPending;
            }
            else
            {
                //delayed payment for company user
                shoppingCartVM.OrderHeader.PaymentStatus = CD.PaymentStatusDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = CD.StatusApproved;
            }

            await unitOfWork.OrderHeader.AddAsync(shoppingCartVM.OrderHeader);
            await unitOfWork.SaveAsync();

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    BookId = cart.BookId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                await unitOfWork.OrderDetail.AddAsync(orderDetail);
                await unitOfWork.SaveAsync();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7175/";
                //make payment with stripe for regular user
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain+$"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach(var item in shoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // Convert to cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Book.Title,
                            },
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                //creating a session
                Session session = service.Create(options);
                unitOfWork.OrderHeader.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                await unitOfWork.SaveAsync();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id  = shoppingCartVM.OrderHeader.Id});
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await unitOfWork.OrderHeader.GetByIdAsync(
                x => x.Id == id,includeProperties:"ApplicationUser");

            if (orderHeader.PaymentStatus != CD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeader.UpdateOrderStatus(id, CD.StatusApproved, CD.PaymentStatusApproved);
                    await unitOfWork.SaveAsync();
                }
                HttpContext.Session.Clear();
            }

            var carts = await unitOfWork.ShoppingCart
            .GetAll(x => x.ApplicationUserId == orderHeader.ApplicationUserId);

            List<ShoppingCart> shoppingCarts = carts.ToList();

            unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
            await unitOfWork.SaveAsync();
            HttpContext.Session.SetInt32(CD.SessionCart, 0);

            return View(id);
        }


        private double CalculateTotal(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Book.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Book.Price50;
                }
                else
                {
                    return shoppingCart.Book.Price100;
                }
            }
        }

        private async Task UpdateSessionCartCount()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var cartItems = await unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId);
            HttpContext.Session.SetInt32(CD.SessionCart, cartItems.Count());
        }


        public async Task<IActionResult> Plus(int cartId)
        {
            var cartFromDB = await unitOfWork.ShoppingCart.GetByIdAsync(u => u.Id == cartId);

            if (cartFromDB == null)
            {
                return NotFound();
            }

            cartFromDB.Count += 1;
            unitOfWork.ShoppingCart.Update(cartFromDB);

            await unitOfWork.SaveAsync();
            await UpdateSessionCartCount();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Minus(int cartId)
        {
            var cartFromDB = await unitOfWork.ShoppingCart.GetByIdAsync(u => u.Id == cartId, tracked: true);

            if (cartFromDB == null) return NotFound();

            if (cartFromDB.Count <= 1)
            {
                //remove from cart
                unitOfWork.ShoppingCart.Delete(cartFromDB);
            }
            else
            {
                cartFromDB.Count -= 1;
                unitOfWork.ShoppingCart.Update(cartFromDB);
            }


            await unitOfWork.SaveAsync();
            await UpdateSessionCartCount();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cartFromDB = await unitOfWork.ShoppingCart.GetByIdAsync(u => u.Id == cartId,tracked:true);

            if (cartFromDB == null) return NotFound();

            unitOfWork.ShoppingCart.Delete(cartFromDB);
            await unitOfWork.SaveAsync();
            await UpdateSessionCartCount();
            return RedirectToAction(nameof(Index));
        }

    }
}
