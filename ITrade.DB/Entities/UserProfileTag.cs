using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class UserProfileTag
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
