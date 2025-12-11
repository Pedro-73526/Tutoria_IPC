using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutoringApp.Data;

namespace TutoringApp.Models
{
    [Table("Years")]
    public class Year
    {
        public int YearId { get; set; }
        public string YearAcademic { get; set; }
        public bool IsActive { get; set; }

        // Relação one to many (Enrollement)
        public List<Enrollement>? Enrollements { get; set; }
        public Year()
        {
            YearAcademic = YearId.ToString() + "/" + (YearId+1).ToString();
            IsActive = false;
        }
    }
}
