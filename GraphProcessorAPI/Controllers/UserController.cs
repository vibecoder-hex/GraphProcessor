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
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        
        public UserController(ILogger<UserController> logger, ILoginService loginService, IUserRepository userRepository, IRegistrationService registrationService, IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService)
        {
            _logger = logger;
            _loginService = loginService;
            _userRepository = userRepository;
            _registrationService = registrationService;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDto userData)
        {
            var loginResult = await _loginService.Login(userData.Username, userData.Password);
            if (!loginResult.IsValid)
            {
                _logger.LogError($"Login failed for user {userData.Username}");
                return Unauthorized(new { Error = loginResult.ErrorMessage});
            }
            _logger.LogInformation($"refresh token {loginResult.RefreshToken.Token}");

            var refreshCookie = new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", loginResult.RefreshToken.Token, refreshCookie);
            
            return Ok(new { TokenString = loginResult.AccessTokenString });
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] UserRegistrationDto userData)
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
        
        [HttpGet("refresh")]
        public async Task<ActionResult> Refresh()
        {
            string? cookieRefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(cookieRefreshToken))
                return Unauthorized(new { Error = "Refresh token is empty" });
            
            var accountRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(cookieRefreshToken);
            if (accountRefreshToken == null || DateTime.UtcNow > accountRefreshToken.ExpiresAt)
                return Unauthorized(new { Error = "Invalid refresh or expired token" });
            
            _logger.LogInformation($"User: {accountRefreshToken.User}");

            var newRefreshToken = await _tokenService.CreateRefreshToken(accountRefreshToken.User);
            
            var refreshCookie = new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, refreshCookie);
            
            string newAccessToken = _tokenService.GetJsonWebTokenString(accountRefreshToken.User);
            return Ok(new { Token = newAccessToken });
        }
        
        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            string? cookieRefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(cookieRefreshToken))
                return BadRequest(new { Error = "Token does not found in cookie" });

            var logoutResult = await _loginService.Logout(cookieRefreshToken);
            if (logoutResult.IsValid)
            {
                Response.Cookies.Delete("refreshToken");
                return Ok("Logout successful");
            }
            return Unauthorized(new { Error = logoutResult.ErrorMessage });
        }
    }
}
