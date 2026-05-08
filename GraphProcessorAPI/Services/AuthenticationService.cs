using GraphProcessorAPI.Models;
using GraphProcessorAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GraphProcessorAPI.Services
{
    public interface ITokenService
    {
        string GetJsonWebTokenString(string username);
    }

    public interface ILoginService
    {
        Task<LoginResult> Login(string username, string password);
    }

    public interface IRegistrationService
    {
        Task<RegistrationResult> Register(string username, string password, string repeatPassword, string firstName, string lastName, string email, string phone);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetJsonWebTokenString(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, UserRole.Admin.ToString())
            };
            var jwtToken = new JwtSecurityToken
                (
                   issuer: _configuration["JwtParams:Issuer"],
                   audience: _configuration["JwtParams:Audience"],
                   claims: claims,
                   expires: DateTime.UtcNow.AddMinutes(30),
                   signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtParams:SecretKey"])), SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }

    public class LoginService : ILoginService
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserRepository _userService;
        private readonly ITokenService _tokenService;

        public LoginService(IPasswordHasher<User> passwordHasher, IUserRepository userService, ITokenService tokenService)
        {
            _passwordHasher = passwordHasher;
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            var userResult = await _userService.GetUserByNameAsync(username);
            if (!userResult.IsValid)
                return new LoginResult { IsValid = false, ErrorMessage = userResult.ErrorMessage };

            var user = userResult.SelectedUser;
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (verificationResult == PasswordVerificationResult.Failed)
                return new LoginResult { IsValid = false, ErrorMessage = "Invalid password"};

            var tokenString = _tokenService.GetJsonWebTokenString(username);
            return new LoginResult { IsValid = true, TokenString = tokenString };
        }
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public RegistrationService(IPasswordHasher<User> passwordHasher, IUserRepository userRepository, ITokenService tokenService)
        {
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        private bool IsValidPasswords(string password, string repeatPassword)
        {
            return (password == repeatPassword) && (password.Length >= 6);
        }

        public async Task<RegistrationResult> Register(string username, string password, string repeatPassword, string firstName, string lastName, string email, string phone)
        {
            var existingUserResult = await _userRepository.GetUserByNameAsync(username);
            if (existingUserResult.IsValid)
                return new RegistrationResult { IsValid = false, ErrorMessage = $"User by {username} is already exists" };

            if (!IsValidPasswords(password, repeatPassword))
                return new RegistrationResult { IsValid = false, ErrorMessage = "Passwords is incorrect" };

            string passwordHash = _passwordHasher.HashPassword(null, password);

            var userCreationResult = await _userRepository.AddUserAsync(username, passwordHash, firstName, lastName, email, phone);
            if (!userCreationResult.IsValid)
                return new RegistrationResult { IsValid = false, ErrorMessage = userCreationResult.ErrorMessage };

            string tokenString = _tokenService.GetJsonWebTokenString(username);
            return new RegistrationResult { IsValid = true, TokenString = tokenString };
        }
    }
}
