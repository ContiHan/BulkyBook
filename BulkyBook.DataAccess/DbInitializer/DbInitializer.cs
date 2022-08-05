using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize()
        {
            // migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }

            // create roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.RoleAdmin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleEmployee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleUserCompany)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleUserIndividual)).GetAwaiter().GetResult();

                // if roles are not created, then create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Name = "Daniel Hanák",
                    PhoneNumber = "123 456 789",
                    StreetAddress = "Hrdinů 1452",
                    State = "Ostravský",
                    PostalCode = "98765",
                    City = "Ostrava"
                }, "Asd.123").GetAwaiter().GetResult();

                var adminUser = _db.ApplicationUsers.FirstOrDefault(a => a.Email == "admin@admin.com");
                _userManager.AddToRoleAsync(adminUser, SD.RoleAdmin).GetAwaiter().GetResult();
            }
        }
    }
}
