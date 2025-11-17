using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; } = null!;
    }
}
