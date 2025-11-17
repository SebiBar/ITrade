using System.ComponentModel.DataAnnotations;

namespace ITrade.DB.Entities
{
    public class ProjectRequest
    {
        [Key]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ProjectRequestTypeId { get; set; }
        public ProjectRequestType ProjectRequestType { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}
