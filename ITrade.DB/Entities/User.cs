using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public string Email { get; set; } = null!;
        [MaxLength(200)]
        public string PasswordHash { get; set; } = null!;
        [MaxLength(200)]
        public string Username { get; set; } = null!;
        [MaxLength(200)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsEmailConfirmed { get; set; } = false;
        public bool Has2FA { get; set; } = false;
        public int UserRoleId { get; set; } 
        public UserRole UserRole { get; set; } = null!;
        public UserMatchingPreferences? MatchingPreferences { get; set; }
        public ICollection<Token> Tokens { get; set; } = [];
        public ICollection<Project> OwnedProjects { get; set; } = [];
        public ICollection<Project> AssignedProjects { get; set; } = [];
        public ICollection<Request> SentRequests { get; set; } = [];
        public ICollection<Request> ReceivedRequests { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<UserProfileLink> UserProfileLinks { get; set; } = [];
        public ICollection<UserProfileTag> UserProfileTags { get; set; } = [];
        public ICollection<Review> SentReviews { get; set; } = [];
        public ICollection<Review> ReceivedReviews { get; set; } = [];
    }
}
