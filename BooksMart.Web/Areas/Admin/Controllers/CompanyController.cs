using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using BooksMart.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksMart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =CD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var companies = (await unitOfWork.Company.GetAll()).ToList();
            return View(companies);
        }
        public async Task<IActionResult> UpsertCompany(int? id)
        {
            if (id==null || id==0)
            {
                return View(new Company());
            }
            else
            {
                Company company = await unitOfWork.Company.GetByIdAsync(u => u.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpsertCompany(Company company)
        {
            if (!ModelState.IsValid)
            {
                return View(company);
            }
            else
            {
                if (company.Id == 0)
                {
                    await unitOfWork.Company.AddAsync(company);
                }
                else
                {
                    unitOfWork.Company.Update(company);
                }
                await unitOfWork.SaveAsync();
                TempData["success"] = "Company Added Successfully";
                return RedirectToAction("Index");
            }
        }
        #region API_CALLS
        [HttpGet]
        public async Task <IActionResult> GetAllCompany()
        {
             List<Company> companies = (await unitOfWork.Company.GetAll()).ToList();
            return Json(new { data = companies});
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var companyToBeDeleted = await unitOfWork.Company.GetByIdAsync(x => x.Id == id);
            if (companyToBeDeleted == null) 
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            unitOfWork.Company.Delete(companyToBeDeleted);
            await unitOfWork.SaveAsync();
            return Json(new {success = true, message = "Deleted Successfully"});
        }
        #endregion
    }
}
