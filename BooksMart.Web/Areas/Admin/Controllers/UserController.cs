using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;
using BooksMart.Models.ViewModels;
using BooksMart.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BooksMart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =CD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserController(UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork,RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var companies = await unitOfWork.Company.GetAll();
            RoleManagementVM roleManagementVM = new()
            {
                ApplicationUser = await unitOfWork.ApplicationUser.GetByIdAsync(u => u.Id == userId,includeProperties:"Company"),
                RolesList = roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name,
                }),
                CompaniesList = companies
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToList(),
            };
            roleManagementVM.ApplicationUser.Role = userManager.GetRolesAsync(await unitOfWork.ApplicationUser
                .GetByIdAsync(r => r.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();

            return View(roleManagementVM);
        }
        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(RoleManagementVM roleManagementVM)
        {
            string prevRole =  userManager.GetRolesAsync(await unitOfWork.ApplicationUser
                .GetByIdAsync(r => r.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser user = await unitOfWork.ApplicationUser.GetByIdAsync(u => u.Id == roleManagementVM.ApplicationUser.Id);
            if (!(roleManagementVM.ApplicationUser.Role==prevRole))
            {
                //here logic for role update
               if (roleManagementVM.ApplicationUser.Role==CD.Role_Company)
                {
                    user.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if (prevRole == CD.Role_Company)
                {
                    user.CompanyId = null;
                }
                unitOfWork.ApplicationUser.Update(user);
                await unitOfWork.SaveAsync();

                userManager.RemoveFromRoleAsync(user, prevRole).GetAwaiter().GetResult();
                userManager.AddToRoleAsync(user, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(prevRole == CD.Role_Company && user.CompanyId != roleManagementVM.ApplicationUser.CompanyId)
                {
                    user.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    unitOfWork.ApplicationUser.Update(user);
                    await unitOfWork.SaveAsync();
                }
            }

            return RedirectToAction("Index");
        }



        #region API_CALLS
        [HttpGet]
        public async Task <IActionResult> GetAllUsers()
        {

            var users = await unitOfWork.ApplicationUser.GetAll(includeProperties: "Company");

            List<ApplicationUser> usersList = users.ToList();

            foreach (var user in users)
            {

                user.Role = userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();


                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = users});
        }
        [HttpPost]
        public async Task<IActionResult> LockUnlockUser([FromBody] string id)
        {
            var objFromDb = await unitOfWork.ApplicationUser.GetByIdAsync(u => u.Id == id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            objFromDb.LockoutEnabled = true;

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.UtcNow)
            {
                // UNLOCK
                objFromDb.LockoutEnd = DateTime.UtcNow;
            }
            else
            {
                // LOCK
                objFromDb.LockoutEnd = DateTime.UtcNow.AddYears(100);
            }
            unitOfWork.ApplicationUser.Update(objFromDb);
            await unitOfWork.SaveAsync();

            return Json(new
            {
                success = true,
                message = "User account status updated successfully."
            });
        }

        #endregion
    }
}
