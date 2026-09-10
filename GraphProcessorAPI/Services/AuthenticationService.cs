using GraphProcessorAPI.Models;
using GraphProcessorAPI.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GraphProcessorAPI.Services
{
    public interface ITokenService
    {
        string GetJsonWebTokenString(User user);
        Task<RefreshToken> CreateRefreshToken(User user);
    }

    public interface ILoginService
    {
        Task<LoginResult> Login(string username, string password);
        Task<LoginResult> Logout(string refreshTokenString);
    }

    public interface IRegistrationService
    {
        Task<RegistrationResult> Register(string username, string password, string repeatPassword, string firstName, string lastName, string email, string phone);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(IConfiguration configuration,  IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public string GetJsonWebTokenString(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };
            var jwtToken = new JwtSecurityToken
                (
                   issuer: _configuration["JwtParams:Issuer"],
                   audience: _configuration["JwtParams:Audience"],
                   claims: claims,
                   expires: DateTime.UtcNow.AddSeconds(30),
                   signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtParams:SecretKey"])), SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public async Task<RefreshToken> CreateRefreshToken(User user)
        {
            Span<byte> byteBuffer = stackalloc byte[32];
            RandomNumberGenerator.Fill(byteBuffer);
            string tokenString = Convert.ToBase64String(byteBuffer);
            var createdToken = await _refreshTokenRepository.AddRefreshTokenAsync(user.UserId, tokenString);
            return createdToken;
        }
    }

    public class LoginService : ILoginService
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserRepository _userService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;

        public LoginService(IPasswordHasher<User> passwordHasher, IUserRepository userService, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
        {
            _passwordHasher = passwordHasher;
            _userService = userService;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            var user = await _userService.GetUserByNameAsync(username);
            if (user == null)
                return new LoginResult { IsValid = false, ErrorMessage = $"User by username {username} not found" };
            
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (verificationResult == PasswordVerificationResult.Failed)
                return new LoginResult { IsValid = false, ErrorMessage = "Invalid password"};

            var accessTokenString = _tokenService.GetJsonWebTokenString(user);
            var refreshToken = await _tokenService.CreateRefreshToken(user);
            
            return new LoginResult { IsValid = true, AccessTokenString = accessTokenString, RefreshToken =  refreshToken };
        }

        public async Task<LoginResult> Logout(string refreshTokenString)
        {
            var refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshTokenString);
            if (refreshToken == null)
                return new LoginResult { IsValid =  false, ErrorMessage = "Refresh token not found" };
            
            await _refreshTokenRepository.RevokeRefreshTokenAsync(refreshToken.Token);
            return new LoginResult { IsValid = true };
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
            if (!IsValidPasswords(password, repeatPassword))
                return new RegistrationResult { IsValid = false, ErrorMessage = "Passwords is incorrect" };
            
            var existingUser = await _userRepository.GetUserByNameAsync(username);
            if (existingUser != null)
                return new RegistrationResult { IsValid = false, ErrorMessage = $"User by {username} is already exists" };
            

            string passwordHash = _passwordHasher.HashPassword(null, password);

            var createdUser = await _userRepository.AddUserAsync(username, passwordHash, firstName, lastName, email, phone);

            string tokenString = _tokenService.GetJsonWebTokenString(createdUser);
            return new RegistrationResult { IsValid = true, TokenString = tokenString };
        }
    }
}
