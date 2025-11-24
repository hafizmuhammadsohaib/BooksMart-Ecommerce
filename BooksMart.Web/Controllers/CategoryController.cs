using BooksMart.Data.Data;
using BooksMart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksMart.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public CategoryController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await dbContext.Categories.ToListAsync();
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
                await dbContext.Categories.AddAsync(category);
                await dbContext.SaveChangesAsync();
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
            var categoryFromDb = await dbContext.Categories.FindAsync(id);
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
                dbContext.Categories.Update(category);
                await dbContext.SaveChangesAsync();
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
            var categoryFromDb = await dbContext.Categories.FindAsync(id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("DeleteCategory")]
        public async Task<IActionResult> DeleteCategoryById(int? id)
        {
            Category? category = await dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
