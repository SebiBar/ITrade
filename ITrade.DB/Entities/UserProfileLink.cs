using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class UserProfileLink
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(2048)]
        public required string Url { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
