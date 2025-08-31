using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoveBudget.API.Data;
using MoveBudget.API.Services;

namespace MoveBudget.Tests
{
    public class AuthServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            return new AppDbContext(options);
        }

        private IConfiguration GetFakeConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "JwtKey", "minha-chave-super-secreta-para-testes" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var config = GetFakeConfiguration();

            // cria usuário fake
            var user = new User
            {
                Username = "testeUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var authService = new AuthService(dbContext, config);

            var loginDto = new LoginDTO
            {
                Username = "teste",
                Password = "123456"
            };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsInvalid()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var config = GetFakeConfiguration();

            var user = new User
            {
                Username = "teste",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var authService = new AuthService(dbContext, config);

            var loginDto = new LoginDTO
            {
                Username = "teste",
                Password = "senha_errada"
            };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }
    }
}