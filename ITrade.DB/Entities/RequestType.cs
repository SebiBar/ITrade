using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class RequestType
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }
    }
}
