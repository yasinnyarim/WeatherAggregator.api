using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WeatherAggregator.Api.Models;
using WeatherAggregator.Api.Models.Dtos;

namespace WeatherAggregator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private static readonly List<User> _users = new();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_users.Count == 0)
            {
                _users.Add(new User { Id = 1, Username = "test@test.com", Password = "password123" });
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDto req)
        {
            if (_users.Exists(u => u.Username.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return StatusCode(409, new { message = "User with this email already exists." });
            }
            var user = new User { Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1, Username = req.Email, Password = req.Password };
            _users.Add(user);
            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto req)
        {
            var user = _users.Find(u => u.Username == req.Email && u.Password == req.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }
            string tokenString = CreateToken(user);
            return Ok(new { token = tokenString });
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, user.Username), new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}