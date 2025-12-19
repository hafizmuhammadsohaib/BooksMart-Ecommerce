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
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public UserController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            this.userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            string RoleId= _dbContext.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

            RoleManagementVM roleManagementVM = new()
            {
                ApplicationUser = await _dbContext.applicationUsers.Include(t=>t.Company).FirstOrDefaultAsync(u => u.Id == userId),
                RolesList = _dbContext.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name,
                }),
                CompaniesList = _dbContext.Companies.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                }),
            };
            roleManagementVM.ApplicationUser.Role = _dbContext.Roles.FirstOrDefault(r => r.Id == RoleId).Name;

            return View(roleManagementVM);
        }
        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(RoleManagementVM roleManagementVM)
        {
            string RoleId = _dbContext.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string prevRole = _dbContext.Roles.FirstOrDefault(r => r.Id == RoleId).Name;

            if (!(roleManagementVM.ApplicationUser.Role==prevRole))
            {
                //here logic for role update
                ApplicationUser user = await _dbContext.applicationUsers.FirstOrDefaultAsync(u => u.Id == roleManagementVM.ApplicationUser.Id);
                if (roleManagementVM.ApplicationUser.Role==CD.Role_Company)
                {
                    user.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if (prevRole == CD.Role_Company)
                {
                    user.CompanyId = null;
                }
                await _dbContext.SaveChangesAsync();

                userManager.RemoveFromRoleAsync(user, prevRole).GetAwaiter().GetResult();
                userManager.AddToRoleAsync(user, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();


            }

            return RedirectToAction("Index");
        }



        #region API_CALLS
        [HttpGet]
        public async Task <IActionResult> GetAllUsers()
        {
             List<ApplicationUser> users = await _dbContext.applicationUsers.Include(c=>c.Company).ToListAsync();
            var userRoles = _dbContext.UserRoles.ToList();
            var roles = _dbContext.Roles.ToList();
            foreach (var user in users)
            {

                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;


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
            var objFromDb = await _dbContext.applicationUsers
                .FirstOrDefaultAsync(u => u.Id == id);

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

            await _dbContext.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "User account status updated successfully."
            });
        }

        #endregion
    }
}
