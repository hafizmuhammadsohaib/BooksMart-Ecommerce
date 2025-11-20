using Microsoft.AspNetCore.Mvc;

namespace BooksMart.Web.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
