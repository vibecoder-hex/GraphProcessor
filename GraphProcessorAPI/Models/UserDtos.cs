using System.ComponentModel.DataAnnotations;

namespace GraphProcessorAPI.Models
{
    public record UserLoginDto(string Username, string Password);
    public record UserRegistrationDto(
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Username must be between 10 and 100 characters long.")]
        string Username,
        string Password,
        string RepeatPassword,
        string FirstName,
        string LastName,
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        string Email,
        [Phone(ErrorMessage = "Invalid Phone Number")]
        string Phone
        );

    public class UserProfileDto
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
