using GraphProcessorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphProcessorAPI.Data
{
    public interface IUserRepository
    {
        Task<UserResult> GetUserByNameAsync(string username);
        Task<UserResult> AddUserAsync(string username, string passwordHash, string firstName, string lastName, string email, string phone);
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

        public async Task<UserResult> GetUserByNameAsync(string username)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return new UserResult { IsValid = false, ErrorMessage = $"User by {username} not found" };

            _logger.LogInformation($"Successfull selected {user} by {username}");
            return new UserResult { IsValid = true, SelectedUser = user };
        }

        public async Task<UserResult> AddUserAsync(string username, string passwordHash, string firstName, string lastName, string email, string phone)
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
            return new UserResult { IsValid = true, SelectedUser = newUser };
        }
    }
}