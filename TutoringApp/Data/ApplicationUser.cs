using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TutoringApp.Models;

namespace TutoringApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(20)]
        public string FirstName { get; set; }

        [StringLength(20)]
        public string LastName { get; set; }

        public int? Number { get; set; }
        public string? NumberMec { get; set; }

        // Relacao one to many
        public List<Enrollement>? Enrollements { get; set; }

        public ApplicationUser()
        {
            FirstName = "";
            LastName = "";
        }

    }
}
 