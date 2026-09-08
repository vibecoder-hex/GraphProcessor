using GraphProcessorAPI.Services;
using GraphProcessorAPI.Models;
using GraphProcessorAPI.Repositories;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace GraphProcessorTest.UserServices
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        private readonly Mock<ITokenService> _mockTokenService = new Mock<ITokenService>();
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

        private readonly ILoginService _loginService;
        private readonly IRegistrationService _registrationService;

        public AuthenticationServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _loginService = new LoginService(_mockPasswordHasher.Object, _mockUserRepository.Object, _mockTokenService.Object, _mockRefreshTokenRepository.Object);
            _registrationService = new RegistrationService(_mockPasswordHasher.Object, _mockUserRepository.Object, _mockTokenService.Object);
        }

        [Fact]
        public async Task LoginTest()
        {
            var user = new User
            {
                Username = "vibecoderhex",
                FirstName = "Simon",
                LastName = "Babushkin",
                CreatedAt = new DateOnly(2026, 5, 2),
                PasswordHash = "AQAAAAIAAYagAAAAEAGonv99quIdG961Lyo9pkCqGdPCoeEeRFujpiWL1s2zgTIYRzbAIu+YWYNqP7A0JA==",
                Email = "rty.sem@yandex.ru",
                Role = UserRole.Admin,
                IsActive = true,
                Phone = "+79251627733"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByNameAsync(user.Username))
                .ReturnsAsync(user);
            _mockPasswordHasher.Setup(hasher => hasher.VerifyHashedPassword(user, user.PasswordHash, "azsxdcQ1!"))
                .Returns(PasswordVerificationResult.Success);

            var result = await _loginService.Login("vibecoderhex", "azsxdcQ1!");
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task RegistrationTestWhenUserIsExists()
        {
            var username = "newuser";
            var password = "SafePassword123!";
            var fakeHash = "hashed_password_string";
            var fakeToken = "penis";
            
            _mockUserRepository.Setup(repo => repo.GetUserByNameAsync(username))
                .ReturnsAsync(new User { Username =  username });

            var result = await _registrationService.Register(username, password, password, "Simon", "Babushkin", "rty.sem@yandex.ru", "+79251627733");
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task RegistrationTestWhenUserNonExists()
        {
            var username = "newuser";
            var password = "SafePassword123!";
            var fakeHash = "hashed_password_string";
            var fakeToken = "penis";

            _mockUserRepository.Setup(repo => repo.GetUserByNameAsync(username))
                .ReturnsAsync((User?)null);
            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(It.IsAny<User?>(), password))                                                                                             
                .Returns(fakeHash);
            _mockUserRepository.Setup(repo => repo.AddUserAsync(username, fakeHash, "Bibos", "Biven", "rty.sem@yandex.ru", "+79251627733"))
                .ReturnsAsync(It.IsAny<User?>());
            _mockTokenService.Setup(token => token.GetJsonWebTokenString(It.IsAny<User?>()))
                .Returns(fakeToken);

            var result = await _registrationService.Register(username, password, password, "Bibos", "Biven", "rty.sem@yandex.ru", "+79251627733");
            Assert.True(result.IsValid);
        }
    }

    public class RefreshTokenTests
    {
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ITokenService _tokenService;

        public RefreshTokenTests()
        {
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _tokenService = new TokenService(_mockConfiguration.Object, _mockRefreshTokenRepository.Object);
        }
        [Fact]
        public async Task RefreshTokenIfExpired()
        {
            var token = new RefreshToken
            {
                Token = "vBcoTEcv8zUYfjfnOA3KDvh0lX6ukWxNSxdTIYsPdtI=",
                ExpiresAt = new DateTime(2026, 9, 5),
                CreatedAt = new DateTime(2026, 8, 6)
            };
            Assert.True(DateTime.UtcNow > token.ExpiresAt);
        }

        [Fact]
        public async Task RefreshTokenIfNotExpired()
        {
            var token = new RefreshToken
            {
                Token = "vBcoTEcv8zUYfjfnOA3KDvh0lX6ukWxNSxdTIYsPdtI=",
                ExpiresAt = new DateTime(2026, 9, 18),
                CreatedAt = new DateTime(2026, 8, 6)
            };
            Assert.False(DateTime.UtcNow > token.ExpiresAt);
        }

        [Fact]
        public async Task TokenGeneratingAndCreating()
        {
            // 1. Создай реального пользователя
            var user = new User
            {
                UserId = 1,
                Username = "testuser",
                // остальные поля, которые нужны для метода (если они есть)
            };

            // 2. Настрой мок, чтобы он реагировал на вызов с этим пользователем
            _mockRefreshTokenRepository
                .Setup(repo => repo.AddRefreshTokenAsync(user.UserId, It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken { Token = "someGeneratedToken", UserId = user.UserId });

            // 3. Вызови метод с реальным пользователем
            var token = await _tokenService.CreateRefreshToken(user);

            // 4. Проверь, что токен создался
            Assert.NotNull(token);
            Assert.False(string.IsNullOrEmpty(token.Token));
        }
    }
}
