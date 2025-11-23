using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class Token
    {
        [Key]
        public int Id { get; set; }
        public required string TokenStringHash { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TokenTypeId { get; set; }
        public TokenType TokenType { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
