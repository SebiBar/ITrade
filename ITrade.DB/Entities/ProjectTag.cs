using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class ProjectTag
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
