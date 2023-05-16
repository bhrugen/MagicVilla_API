using Microsoft.AspNetCore.Identity;

namespace MagicVilla_Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
