using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DragonBallApi.Controllers
{

    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginModel login)
        {
            _logger.LogInformation("Login attempt for user: {Username}", login.Username);

            if (login.Username == "testuser" && login.Password == "Pa$$w0rd")
            {
                _logger.LogInformation("User {Username} authenticated successfully.", login.Username);
                var token = GenerateJwtToken(login.Username);
                return Ok(new { token });
            }

            _logger.LogWarning("Invalid login attempt for user: {Username}", login.Username);
            return Unauthorized("Invalid credentials");
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials);

            _logger.LogDebug("JWT token generated for user: {Username}", username);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}