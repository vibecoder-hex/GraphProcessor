using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GraphProcessorClient
{

    public record AuthenticationResultDto(string TokenString);
    public record UserLoginDto(string Username, string Password);
    public record UserRegistrationDto(string Username, string Password, string RepeatPassword, string FirstName, string LastName, string Email, string Phone);
    public record UserProfileDto(string Username, string FirstName, string LastName, string Email, string Phone);
    public interface IAuthenticationClient
    {
        Task<AuthenticationResultDto> Login(UserLoginDto dto);
        Task<AuthenticationResultDto> Register(UserRegistrationDto dto);
    }

    public interface IProfileClient
    {
        Task<UserProfileDto> GetProfile();
    }

    public static class TokenProvider
    {
        public static async Task SaveToken(string token, string filePath = "token.txt")
        {
            await File.WriteAllTextAsync(filePath, token);
        }

        public static async Task<string?> GetToken(string filePath = "token.txt")
        {
            if (File.Exists(filePath))
            {
                return await File.ReadAllTextAsync(filePath);
            }
            return null;
        }
    }

    public class AuthenticationClient : IAuthenticationClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AuthenticationClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        private async Task<T> SentPostRequest<T>(string endpoint, object dto)
        {
            StringContent jsonContent = new StringContent(JsonSerializer.Serialize(dto), System.Text.Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", jsonContent);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadFromJsonAsync<T>();
            return responseContent;
        } 

        public async Task<AuthenticationResultDto> Login(UserLoginDto dto)
        {
            return await SentPostRequest<AuthenticationResultDto>("User/login", dto);
        }

        public async Task<AuthenticationResultDto> Register(UserRegistrationDto dto)
        {
            return await SentPostRequest<AuthenticationResultDto>("User/register", dto);
        }
    }

    public class ProfileClient : IProfileClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ProfileClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<UserProfileDto> GetProfile()
        {
            string? token = await TokenProvider.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Token not found. Please login first.");
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/User/profile");
            response.EnsureSuccessStatusCode();
            var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            return profile;
        }
    }

    public class Program
    {
        public static async Task TaskHandleLoginResponse(IAuthenticationClient authClient)
        {
            Console.WriteLine("Enter username and password:");
            string? username = Console.ReadLine();
            string? password = Console.ReadLine();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Username and password cannot be empty.");
                return;
            }
            try
            {
                var authResult = await authClient.Login(new UserLoginDto(username, password));
                await TokenProvider.SaveToken(authResult.TokenString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
            }
        }

        public static async Task TaskHandleRegisterResponse(IAuthenticationClient authClient)
        {
            string? registrationData = Console.ReadLine();
            if (string.IsNullOrEmpty(registrationData))
            {
                Console.WriteLine("Registration data cannot be empty.");
                return;
            }
            string[] dataParts = registrationData.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (dataParts.Length != 7)
            {
                Console.WriteLine("data must contain 7 parts: Username,Password,RepeatPassword,FirstName,LastName,Email,Phone.");
                return;
            }
            var registrationDto = new UserRegistrationDto(dataParts[0], dataParts[1], dataParts[2], dataParts[3], dataParts[4], dataParts[5], dataParts[6]);
            try
            {        
                var authResult = await authClient.Register(registrationDto);
                await TokenProvider.SaveToken(authResult.TokenString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration failed: {ex.Message}");
            }
        }

        public static async Task TaskHandleProfileResponse(IProfileClient profileClient)
        {
            try
            {
                var profile = await profileClient.GetProfile();
                Console.WriteLine($"Username: {profile.Username}");
                Console.WriteLine($"First Name: {profile.FirstName}");
                Console.WriteLine($"Last Name: {profile.LastName}");
                Console.WriteLine($"Email: {profile.Email}");
                Console.WriteLine($"Phone: {profile.Phone}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching profile: {ex.Message}");
            }
        }

        public async static Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            string baseUrl = "http://213.171.29.203/api";
            while (true)
            {
                Console.WriteLine("1. Login\n2. Register\n3. Profile");
                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await TaskHandleLoginResponse(new AuthenticationClient(httpClient, baseUrl));
                        break;
                    case "2":
                        await TaskHandleRegisterResponse(new AuthenticationClient(httpClient, baseUrl));
                        break;
                    case "3":
                        await TaskHandleProfileResponse(new ProfileClient(httpClient, baseUrl));
                        break;
                }
            }
        }
    }
}