using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BooksMart.Web.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(CD.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(CD.SessionCart,
                    await unitOfWork.ShoppingCart.CountAsync(j => j.ApplicationUserId == claim.Value));
                }

                return View(HttpContext.Session.GetInt32(CD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
