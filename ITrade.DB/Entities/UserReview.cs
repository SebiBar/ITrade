using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class UserReview
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public string? Title { get; set; } = null;
        [MaxLength(2000)]
        public string? Comment { get; set; } = null;
        [Range(1,5)]
        public int Rating { get; set; }
        public int ReviewerId { get; set; }
        public User Reviewer { get; set; } = null!;
        public int RevieweeId { get; set; }
        public User Reviewee { get; set; } = null!;
    }
}
