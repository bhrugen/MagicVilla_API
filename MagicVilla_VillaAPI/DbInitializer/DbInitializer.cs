using IdentityModel;
using MagicVilla_Utility;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MagicVilla_VillaAPI.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialize()
        {
            if (_roleManager.FindByNameAsync(SD.Admin).Result == null)
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Customer)).GetAwaiter().GetResult();
            }
            else { return; }

            ApplicationUser adminUser = new()
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "111111111111",
                Name = "Ben Admin",
            };

            _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(adminUser, SD.Admin).GetAwaiter().GetResult();

            var claims = _userManager.AddClaimsAsync(adminUser, new Claim[] {
                new Claim(JwtClaimTypes.Name,adminUser.Name),
                new Claim(JwtClaimTypes.Role,SD.Admin)
            }).Result;



            ApplicationUser customerUser = new()
            {
                UserName = "customer@gmail.com",
                Email = "customer@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "111111111111",
                Name = "Ben Customer",
            };

            _userManager.CreateAsync(customerUser, "Admin123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(customerUser, SD.Customer).GetAwaiter().GetResult();

            var temp2 = _userManager.AddClaimsAsync(customerUser, new Claim[] {
                 new Claim(JwtClaimTypes.Name,customerUser.Name),
                new Claim(JwtClaimTypes.Role,SD.Customer),
            }).Result;
        }
    }
}