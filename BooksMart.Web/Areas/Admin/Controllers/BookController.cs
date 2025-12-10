using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using BooksMart.Models.ViewModels;
using BooksMart.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BooksMart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =CD.Role_Admin)]
    public class BookController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;

        public BookController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var books = (await unitOfWork.Book.GetAll(includeProperties: "Category")).ToList();
            return View(books);
        }
        public async Task<IActionResult> UpsertBook(int? id)
        {
            var categories = await unitOfWork.Category.GetAll();
            IEnumerable<SelectListItem> CategoryList = categories.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            BookVM bookVM = new()
            {
                CategoryList = CategoryList,
                Book = new Book()
            };
            if (id==null || id==0)
            {
                return View(bookVM);
            }
            else
            {
                bookVM.Book = await unitOfWork.Book.GetByIdAsync(u => u.Id == id);
                return View(bookVM);
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpsertBook(BookVM bookVM, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                var categories = await unitOfWork.Category.GetAll();

                bookVM.CategoryList = categories
                    .Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    })
                    .ToList();

                return View(bookVM);
            }
            else
            {
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (file!=null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string bookPath = Path.Combine(wwwRootPath, @"images\books");
                    if (!string.IsNullOrEmpty(bookVM.Book.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(
                            wwwRootPath, bookVM.Book.ImageUrl.TrimStart('\\')
                            );
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(bookPath,fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    bookVM.Book.ImageUrl = @"\images\books\" + fileName;
                }
                if (bookVM.Book.Id == 0)
                {
                    await unitOfWork.Book.AddAsync(bookVM.Book);
                }
                else
                {
                    unitOfWork.Book.Update(bookVM.Book);
                }
                await unitOfWork.SaveAsync();
                TempData["success"] = "Book Added Successfully";
                return RedirectToAction("Index");
            }
        }
        #region API_CALLS
        [HttpGet]
        public async Task <IActionResult> GetAllBooks()
        {
            var books = (await unitOfWork.Book.GetAll(includeProperties: "Category")).ToList();
            return Json(new { data = books});
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var bookToBeDeleted = await unitOfWork.Book.GetByIdAsync(x => x.Id == id);
            if (bookToBeDeleted == null) 
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            var oldImagePath = Path.Combine(
                            webHostEnvironment.WebRootPath, bookToBeDeleted.ImageUrl.TrimStart('\\')
                            );
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            unitOfWork.Book.Delete(bookToBeDeleted);
            await unitOfWork.SaveAsync();
            return Json(new {success = true, message = "Deleted Successfully"});
        }
        #endregion
    }
}
