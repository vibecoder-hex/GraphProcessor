namespace GraphProcessorAPI.Models
{
    public enum UserRole { User, Admin };
    public partial class User
    {
        public int UserId { get; set; }

        public string Username { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateOnly CreatedAt { get; set; }

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string PasswordHash { get; set; } = null!;

        public bool IsActive { get; set; }

        public UserRole Role { get; set; }

        public virtual ICollection<Graph> Graphs { get; set; } = new List<Graph>();
        
        public virtual RefreshToken? RefreshToken { get; set; }
    }

    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        
        public virtual User? User { get; set; }
    }

}
