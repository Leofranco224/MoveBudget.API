using Microsoft.IdentityModel.Tokens;
using MoveBudget.API.Data;
using MoveBudget.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MoveBudget.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<bool> RegisterAsync(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return false;

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LoginResult?> LoginAsync(LoginDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            var jwt = GenerateJwtToken(user);

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new LoginResult
            {
                AccessToken = jwt,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<LoginResult?> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

            if (token == null || token.Expires < DateTime.UtcNow)
                return null;

            token.IsRevoked = true;

            var newJwt = GenerateJwtToken(token.User);

            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = token.UserId
            };
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return new LoginResult
            {
                AccessToken = newJwt,
                RefreshToken = newRefreshToken.Token
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JwtKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}