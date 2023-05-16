using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string JwtTokenId { get; set; }
        public string Refresh_Token { get; set; }
        //We will make sure the refresh token is only valid for one use
        public bool IsValid { get; set; }
        public DateTime ExpiresAt { get; set; }

    }
}
