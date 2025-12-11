using System.ComponentModel.DataAnnotations.Schema;

namespace TutoringApp.Models
{
    [Table("Meetings")]
    public class Meeting
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Subject { get; set; }
        public string? TypeContact { get; set; }
        public string? Description { get; set; }

        // Relação one to many (group to meeting)
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]

        public Group? Group { get; set; }
    }
}
