using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using BooksMart.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BooksMart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
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
                includeProperties: "Book")
            };

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = CalculateTotal(cart);
                shoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        public IActionResult Summary()
        {
            return View();
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
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Minus(int cartId)
        {
            var cartFromDB = await unitOfWork.ShoppingCart.GetByIdAsync(u => u.Id == cartId);

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
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cartFromDB = await unitOfWork.ShoppingCart.GetByIdAsync(u => u.Id == cartId);

            if (cartFromDB == null) return NotFound();

            unitOfWork.ShoppingCart.Delete(cartFromDB);
            await unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
