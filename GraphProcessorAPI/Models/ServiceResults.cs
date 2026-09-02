namespace GraphProcessorAPI.Models
{
    public abstract class ServiceResult
    {
        public required bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class LoginResult : ServiceResult
    {
        public string? AccessTokenString { get; set; }
        public RefreshToken? RefreshToken { get; set; }
    }

    public class RegistrationResult: ServiceResult
    {
        public string? TokenString { get; set; }
    }
}
 