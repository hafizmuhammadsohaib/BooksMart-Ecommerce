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
                bookVM.Book = await unitOfWork.Book.GetByIdAsync(u => u.Id == id,includeProperties: "BookImages");
                return View(bookVM);
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpsertBook(BookVM bookVM, List<IFormFile> files)
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
                if (bookVM.Book.Id == 0)
                {
                    await unitOfWork.Book.AddAsync(bookVM.Book);
                }
                else
                {
                    unitOfWork.Book.Update(bookVM.Book);
                }
                await unitOfWork.SaveAsync();
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (files!=null)
                {

                    foreach (var file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string bookPath = @"images\books\book-" + bookVM.Book.Id;
                        string finalBookPath = Path.Combine(wwwRootPath, bookPath);

                        if (!Directory.Exists(finalBookPath))
                        {
                            Directory.CreateDirectory(finalBookPath);
                        }
                        using (var fileStream = new FileStream(Path.Combine(finalBookPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        BookImage bookImage = new()
                        {
                            ImageUrl = @"\" + bookPath + @"\" + fileName,
                            BookId = bookVM.Book.Id
                        };

                        if (bookVM.Book.BookImages == null)
                        {
                            bookVM.Book.BookImages = new List<BookImage>();
                        }

                        bookVM.Book.BookImages.Add(bookImage);


                    }
                    unitOfWork.Book.Update(bookVM.Book);
                    await unitOfWork.SaveAsync();
                }

                TempData["success"] = "Book Created/Updated Successfully";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var imageToDelete = await unitOfWork.BookImage.GetByIdAsync(u => u.Id == imgId);
            int bookId = imageToDelete.BookId;
            if (imageToDelete != null) {
                if (!string.IsNullOrEmpty(imageToDelete.ImageUrl))
                {
                    var oldImagePath = Path.Combine(
                                    webHostEnvironment.WebRootPath, imageToDelete.ImageUrl.TrimStart('\\')
                                    );
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                unitOfWork.BookImage.Delete(imageToDelete);
                await unitOfWork.SaveAsync();
                TempData["success"] = "Deleted Successfully!";
            }

            return RedirectToAction(nameof(UpsertBook), new { id = bookId });
            
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
            string bookPath = @"images\books\book-" + id;
            string finalBookPath = Path.Combine(webHostEnvironment.WebRootPath, bookPath);

            if (Directory.Exists(finalBookPath))
            {
                string[] filesPath = Directory.GetFiles(finalBookPath);
                foreach (string path in filesPath)
                {
                    System.IO.File.Delete(path);
                }


                Directory.Delete(finalBookPath);
            }

            unitOfWork.Book.Delete(bookToBeDeleted);
            await unitOfWork.SaveAsync();
            return Json(new {success = true, message = "Deleted Successfully"});
        }
        #endregion
    }
}
