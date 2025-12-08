using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksMart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await unitOfWork.Category.GetAll();
            return View(categories);
        }
        public IActionResult CreateCategory()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            //Custom Validations
            if (category.Name==category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Category Name cannot be same as Display Order!");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                await unitOfWork.Category.AddAsync(category);
                await unitOfWork.SaveAsync();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = await unitOfWork.Category.GetByIdAsync(u => u.Id == id);
            //var categoryFromDb1 = await dbContext.Categories.FirstOrDefaultAsync(x=>x.Id==id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Update(category);
                await unitOfWork.SaveAsync();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = await unitOfWork.Category.GetByIdAsync(u => u.Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("DeleteCategory")]
        public async Task<IActionResult> DeleteCategoryById(int? id)
        {
            Category? category = await unitOfWork.Category.GetByIdAsync(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            unitOfWork.Category.Delete(category);
            await unitOfWork.SaveAsync();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
