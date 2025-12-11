using System.Diagnostics;
using System.Security.Claims;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksMart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Book> books = await unitOfWork.Book.GetAll(includeProperties: "Category");
            return View(books);
        }
        public async Task<IActionResult> Details(int id)
        {
            ShoppingCart shoppingCart = new()
            {
                Book = await unitOfWork.Book.GetByIdAsync(u => u.Id == id, includeProperties: "Category"),
                Count = 1,
                BookId = id
            };

            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(int bookId, int count)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCart shoppingCartFromDb = await unitOfWork.ShoppingCart
                .GetByIdAsync(u => u.ApplicationUserId == userId && u.BookId == bookId);

            if (shoppingCartFromDb != null)
            {
                shoppingCartFromDb.Count += count;
                unitOfWork.ShoppingCart.Update(shoppingCartFromDb);
            }
            else
            {
                var newCart = new ShoppingCart
                {
                    ApplicationUserId = userId,
                    BookId = bookId,
                    Count = count
                };
                await unitOfWork.ShoppingCart.AddAsync(newCart);
            }
            TempData["success"] = "Cart Updated Successfully";
            await unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
