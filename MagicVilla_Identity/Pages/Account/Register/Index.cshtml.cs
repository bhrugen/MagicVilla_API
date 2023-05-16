using MagicVilla_Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MagicVilla_Identity.Pages.Account.Register
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleInManager
              )
        {
            _roleManager = roleInManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [BindProperty]
        public RegisterViewModel Input { get; set; }


        public async Task<IActionResult> OnGet(string returnUrl)
        {
            List<string> roles = new()
            {
                SD.Admin,
                SD.Customer
            };
            ViewData["roles_message"] = roles;
            Input = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };
            return Page();
        }
    }
}