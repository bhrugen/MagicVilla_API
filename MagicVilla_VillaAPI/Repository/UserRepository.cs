using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private string secretKey;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration,
            TokenValidationParameters tokenValidationParameters,
            UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            _tokenValidationParameters = tokenValidationParameters;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.ApplicationUsers
                .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);


            if (user == null || isValid == false)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    RefreshToken=""
                };
            }

            TokenRequestDTO tokenRequest = GenerateJwtToken(user);
            LoginResponseDTO loginResponseDTO = new()
            {
                Token = tokenRequest.JwtToken,
                RefreshToken = tokenRequest.RefreshToken,
            };
            return loginResponseDTO;
        }

        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email=registerationRequestDTO.UserName,
                NormalizedEmail=registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult()){
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                    var userToReturn = _db.ApplicationUsers
                        .FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);

                }
            }
            catch(Exception e)
            {

            }

            return new UserDTO();
        }

        public async Task<LoginResponseDTO> GenerateNewTokenFromRefreshToken(TokenRequestDTO tokenRequestDTO)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenVerification = tokenHandler.ValidateToken(tokenRequestDTO.JwtToken, _tokenValidationParameters, out var validatedToken);

            if (validatedToken != null)
            {
                //check our custom validations
                //Get claims from Token
                var jwt = tokenHandler.ReadJwtToken(tokenRequestDTO.JwtToken);
                var name = jwt.Claims.FirstOrDefault(u => u.Type == "name").Value;
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userID = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;

                var refreshTokenFromDb = _db.RefreshToken.FirstOrDefault(u => u.UserId == userID 
                && u.JwtTokenId == jwtTokenId && u.Refresh_Token==tokenRequestDTO.RefreshToken);
                if (refreshTokenFromDb != null)
                {
                    //userid and jti combination is valid
                    if (refreshTokenFromDb.ExpiresAt <= DateTime.Now)
                    {
                        //refresh token expired
                        return new LoginResponseDTO();
                    }
                    if (!refreshTokenFromDb.IsValid)
                    {
                        return new LoginResponseDTO();
                    }
                    ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userID);
                    if (applicationUser != null)
                    {
                        //update old token
                        refreshTokenFromDb.IsValid = false;

                        //generate new token
                        TokenRequestDTO tokenResponse =  GenerateJwtToken(applicationUser);
                        return new LoginResponseDTO()
                        {
                            Token = tokenResponse.JwtToken,
                            RefreshToken = tokenResponse.RefreshToken
                        };
                    }
                }
                else
                {
                    //no refresh token found
                    return new LoginResponseDTO();
                }

            }
            return new LoginResponseDTO();

        }

        private TokenRequestDTO GenerateJwtToken(ApplicationUser user)
        {
            var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti,"JTI"+Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub,user.Id)
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            //if user was found generate JWT Token
            RefreshToken newRefreshToken = new()
            {
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsValid = true,
                JwtTokenId = token.Id,
                UserId = user.Id
            };
            _db.RefreshToken.Add(newRefreshToken);
            _db.SaveChanges();

            return new TokenRequestDTO()
            {
                JwtToken = tokenHandler.WriteToken(token),
                RefreshToken = newRefreshToken.Refresh_Token
            };
            

        }
    }
}
