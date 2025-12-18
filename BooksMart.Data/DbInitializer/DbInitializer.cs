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
                if (applicationDbContext.Database.GetPendingMigrations().Any())
                {
                    applicationDbContext.Database.Migrate();
                }
            }
            catch
            {
                throw;
            }

            if (!roleManager.RoleExistsAsync(CD.Role_Admin).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(CD.Role_Customer)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(CD.Role_Employee)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(CD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(CD.Role_Company)).GetAwaiter().GetResult();
            }

            var adminUser = userManager.FindByEmailAsync("admin@BooksMart.com").GetAwaiter().GetResult();

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@BooksMart.com",
                    Email = "admin@BooksMart.com",
                    Name = "Admin User",
                    PhoneNumber = "111-222-3333",
                    Address = "123 Admin St",
                    City = "Admin City",
                    Province = "Admin Province",
                    PostalCode = "A1A1A1",
                    EmailConfirmed = true
                };

                userManager.CreateAsync(user, "Admin123*").GetAwaiter().GetResult();
                userManager.AddToRoleAsync(user, CD.Role_Admin).GetAwaiter().GetResult();
            }
        }

    }
}