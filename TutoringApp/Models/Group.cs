using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;
using TutoringApp.Data;

namespace TutoringApp.Models
{
    [Table("Groups")]
    public class Group
    {
        public int GroupId { get; set; }

        public int YearId { get; set; }

        // Relacao many to many
        public ICollection<Enrollement> Enrollements { get; set; }
        public List<Meeting>? Meetings { get; set; }
    }
}
