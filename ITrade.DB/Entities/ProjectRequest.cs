using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class ProjectRequest
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(2000)]
        public string? Message { get; set; } = null;
        public bool? Accepted { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ProjectRequestTypeId { get; set; }
        public ProjectRequestType ProjectRequestType { get; set; } = null!;
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}
