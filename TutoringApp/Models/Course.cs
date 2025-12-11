using MessagePack;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TutoringApp.Models
{
    [Table("Courses")]
    public class Course
    {
        public int CourseId { get; set; }
        [Required]
        public int CourseCode { get; set; }
        public string CourseName { get; set; }
        public string School { get; set; }

        // Relação One to Many
        public List<Enrollement>? Enrollements { get; set; }

        public Course()
        {
            CourseName = "";
            School = "";
        }
    }
}