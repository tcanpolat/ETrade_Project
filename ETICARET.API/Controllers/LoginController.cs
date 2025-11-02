using ETICARET.API.Identity;
using ETICARET.API.Models;
using ETICARET.Business.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ETICARET.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ICartService _cartService;

        public LoginController(UserManager<ApplicationUser> userManager,
                               SignInManager<ApplicationUser> signInManager,
                               IConfiguration configuration,
                               ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _cartService = cartService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return BadRequest(ApiResponse<string>.ErrorResponse(errors, "Geçersiz giriş verileri"));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse(new List<string> { "Geçersiz email veya şifre" }, "Giriş Başarısız"));
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);

            if (result.Succeeded)
            {
                var token = await GenerateJwtToken(user);

                var response = new LoginResponse()
                {
                    Token = token,
                    Email = user.Email,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration.GetSection("JWTSettings")["ExpirationInMinutes"]))
                };

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Giriş Başarılı"));
            }

            if (result.IsLockedOut)
            {
                return StatusCode(423,ApiResponse<string>.ErrorResponse(new List<string> { "Hesabınız kilitlenmiştir. Lütfen daha sonra tekrar deneyin." }, "Giriş Başarısız"));
            }

            return Unauthorized(ApiResponse<string>.ErrorResponse(new List<string> { "Geçersiz email veya şifre" }, "Giriş Başarısız"));
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            /* 
             JWT
             Header => Token tipi ve algoritma bilgisi
             Payload => Kullanıcı bilgileri ve token ile ilgili diğer bilgiler
             Signature => İmza bilgisi
             
             */

           var jwtSettings = _configuration.GetSection("JWTSettings");
           var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Jwt Secret Key yapılandırması bulunamadı");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim("FullName", user.FullName ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Token Oluşturma

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
