using GraphProcessorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphProcessorAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByNameAsync(string username);
        Task<User?> AddUserAsync(string username, string passwordHash, string firstName, string lastName, string email, string phone);
    }

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> AddRefreshTokenAsync(int userId, string refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string tokenString);
    }

    public class UserRepository : IUserRepository
    {
        private readonly GraphProcessorContext _dbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(GraphProcessorContext dbContext, ILogger<UserRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<User?> GetUserByNameAsync(string username)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            _logger.LogInformation($"Successfull selected {user} by {username}");
            return user;
        }

        public async Task<User?> AddUserAsync(string username, string passwordHash, string firstName, string lastName, string email, string phone)
        {
            var newUser = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                IsActive = true,
                Role = UserRole.Admin,
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),

            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return newUser;
        }
    }

    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly GraphProcessorContext _dbContext;
        
        public  RefreshTokenRepository(GraphProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> AddRefreshTokenAsync(int userId, string refreshToken)
        {
            var existingToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);
            if (existingToken != null)
            {
                existingToken.Token = refreshToken;
                existingToken.ExpiresAt = DateTime.UtcNow.AddDays(7);
                existingToken.CreatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return existingToken;
            }
            
            var newToken = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            
            _dbContext.Add(newToken);
            await _dbContext.SaveChangesAsync();
            return newToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenString)
        {
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == tokenString);
            return token;
        }
    }
}