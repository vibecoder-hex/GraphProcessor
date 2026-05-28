using GraphProcessorAPI.Models;
using GraphProcessorAPI.Services;
using GraphProcessorAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace GraphProcessorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ILoginService _loginService;
        private readonly IRegistrationService _registrationService;
        private readonly IUserRepository _userRepository;
        
        public UserController(ILogger<UserController> logger, ILoginService loginService, IUserRepository userRepository, IRegistrationService registrationService)
        {
            _logger = logger;
            _loginService = loginService;
            _userRepository = userRepository;
            _registrationService = registrationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userData)
        {
            var loginResult = await _loginService.Login(userData.Username, userData.Password);
            if (!loginResult.IsValid)
            {
                _logger.LogError($"Login failed for user {userData.Username}");
                return Unauthorized(new { Error = loginResult.ErrorMessage});
            }
            _logger.LogInformation($"User {userData.Username} logged in and received token: {loginResult.TokenString}");
            return Ok(new { TokenString = loginResult.TokenString });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userData)
        {
            var registerResult = await _registrationService.Register(userData.Username, userData.Password, userData.RepeatPassword, userData.FirstName, userData.LastName, userData.Email, userData.Phone);
            if (!registerResult.IsValid)
            {
                _logger.LogError($"Registration filed");
                return Unauthorized(new { Error = registerResult.ErrorMessage  });
            }
            _logger.LogInformation($" {userData.Username} registered and recieved token: { registerResult.TokenString } ");
            return Ok(new { TokenString = registerResult.TokenString  });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult> Profile()
        {
            string? username = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { Error = "Username does not found in http context" });

            var user = await _userRepository.GetUserByNameAsync(username);
            if (user == null)
                return Unauthorized(new { Error = "User not found" });
            
            var userProfile = new UserProfileDto
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.Phone,
                Email = user.Email
            };

            return Ok(userProfile);
        }
    }
}
