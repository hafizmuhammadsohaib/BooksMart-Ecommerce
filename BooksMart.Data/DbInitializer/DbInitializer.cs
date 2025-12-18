using BooksMart.Data.Data;
using BooksMart.Models.Models;
using BooksMart.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksMart.Data.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext applicationDbContext;

        public DbInitializer(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext applicationDbContext
            )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.applicationDbContext = applicationDbContext;
        }

        public void Initialize()
        {
            try
            {
                if (applicationDbContext.Database.GetPendingMigrations().Count() > 0)
                {
                    applicationDbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            if (!roleManager.RoleExistsAsync(CD.Role_Customer).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(CD.Role_Customer)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(CD.Role_Employee)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(CD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(CD.Role_Company)).GetAwaiter().GetResult();



                userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@BooksMart.com",
                    Email = "admin@BooksMart.com",
                    Name = "Admin User",
                    PhoneNumber = "111-222-3333",
                    Address = "123 Admin St",
                    City = "Admin City",
                    Province = "Admin Province",
                    PostalCode = "A1A1A1"
                }, "Admin123*").GetAwaiter().GetResult();

                ApplicationUser user = applicationDbContext.applicationUsers.FirstOrDefault(x => x.Email == "admin@BooksMart.com");
                userManager.AddToRoleAsync(user, CD.Role_Admin).GetAwaiter().GetResult();


            }
            return;
        }
    }
}