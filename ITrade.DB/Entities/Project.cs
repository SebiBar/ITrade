using ITrade.DB.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public required string Name { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; } = null;
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public int ProjectStatusTypeId { get; set; } = (int)ProjectStatusTypeEnum.Hiring;
        public ProjectStatusType ProjectStatusType { get; set; } = null!;
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public int? WorkerId { get; set; } = null;
        public User? Worker { get; set; } = null;
        public ICollection<ProjectTag> ProjectTags { get; set; } = [];
        public ICollection<Request> Requests { get; set; } = [];
    }
}
