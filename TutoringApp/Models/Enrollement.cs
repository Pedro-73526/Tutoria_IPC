using System.ComponentModel.DataAnnotations.Schema;
using TutoringApp.Data;

namespace TutoringApp.Models
{
    public class Enrollement
    {
        public int Id { get; set; }
        public string Role { get; set; }

        // Relacao one to many (User)
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        // Relacao one to many (Year)
        public int YearId { get; set; }
        [ForeignKey("YearId")]
        public Year Year { get; set; }

        // Relacao many to many (Group)
        public ICollection<Group>? Groups { get; set; }

        // Relacao one to many (Course)
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
