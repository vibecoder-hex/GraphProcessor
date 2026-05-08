using GraphProcessorAPI.Services;
using GraphProcessorAPI.Models;
using GraphProcessorAPI.Data;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace GraphProcessorTest.UserServices
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        private readonly Mock<ITokenService> _mockTokenService = new Mock<ITokenService>();

        private readonly ILoginService _loginService;
        private readonly IRegistrationService _registrationService;

        public AuthenticationServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _loginService = new LoginService(_mockPasswordHasher.Object, _mockUserRepository.Object, _mockTokenService.Object);
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
                .ReturnsAsync(new UserResult { IsValid = true, SelectedUser = user });
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

            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(null, password))
                .Returns(fakeHash);
            _mockUserRepository.Setup(repo => repo.GetUserByNameAsync(username))
                .ReturnsAsync(new UserResult { IsValid = true });
            _mockUserRepository.Setup(repo => repo.AddUserAsync(username, fakeHash, "Simon", "Babushkin", "rty.sem@yandex.ru", "+79251627733"))
                .ReturnsAsync(new UserResult { IsValid = true });
            _mockTokenService.Setup(token => token.GetJsonWebTokenString(fakeHash))
                .Returns(fakeToken);

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

            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(null, password))                                                                                             
                .Returns(fakeHash);
            _mockUserRepository.Setup(repo => repo.GetUserByNameAsync(username))
                .ReturnsAsync(new UserResult { IsValid = false });
            _mockUserRepository.Setup(repo => repo.AddUserAsync(username, fakeHash, "Bibos", "Biven", "rty.sem@yandex.ru", "+79251627733"))
                .ReturnsAsync(new UserResult { IsValid = true });
            _mockTokenService.Setup(token => token.GetJsonWebTokenString(username))
                .Returns(fakeToken);

            var result = await _registrationService.Register(username, password, password, "Bibos", "Biven", "rty.sem@yandex.ru", "+79251627733");
            Assert.True(result.IsValid);
        }
    }
}
