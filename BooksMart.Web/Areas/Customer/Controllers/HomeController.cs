using System.Diagnostics;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
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
            Book? book = await unitOfWork.Book.GetByIdAsync(
                u => u.Id == id,
                includeProperties: "Category"
            );

            if (book == null)
                return NotFound();

            return View(book);
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
