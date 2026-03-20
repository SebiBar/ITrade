using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class UserMatchingPreferences
    {
        [Key]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int TagMatchMaxPercentage { get; set; } = 60;
        public int ExperienceMaxPercentage { get; set; } = 20;
        public int ReviewsMaxPercentage { get; set; } = 20;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
