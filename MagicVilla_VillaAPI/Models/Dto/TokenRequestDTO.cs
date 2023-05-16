using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models.Dto
{
    public class TokenRequestDTO
    {
        [Required]
        public string JwtToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
