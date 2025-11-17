using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class SeedStatus
    {
        [Key]
        public int Id { get; set; }
        public bool ShouldSeedDatabase { get; set; }
    }
}
