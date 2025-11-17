using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public required string Name { get; set; }
        [MaxLength(2000)]
        public required string Content { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
