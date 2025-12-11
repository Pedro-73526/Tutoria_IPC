using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TutoringApp.Models;

namespace TutoringApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Course> Courses { get; set; }    
        public DbSet<Year> Years { get; set; }
        public DbSet<Enrollement> Enrollements { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>()

                .Ignore(c => c.AccessFailedCount)
                .Ignore(c => c.LockoutEnabled)
                .Ignore(c => c.TwoFactorEnabled)
                .Ignore(c => c.ConcurrencyStamp)
                .Ignore(c => c.LockoutEnd)
                .Ignore(c => c.EmailConfirmed)
                .Ignore(c => c.TwoFactorEnabled)
                .Ignore(c => c.LockoutEnd)
                .Ignore(c => c.PhoneNumberConfirmed);


            // Nao posso alterar pois tinha que ter alterado tambem outras migracoes anteriormente.
            modelBuilder.Entity<ApplicationUser>().ToTable("Users"); // to change the name of table.

            //var hasher = new PasswordHasher<ApplicationUser>();

            //// Seed default user
            //var adminUser = new ApplicationUser
            //{
            //    Id = "1",
            //    UserName = "tutoria@utad.pt",
            //    NormalizedUserName = "TUTORIA@UTAD.PT",
            //    Email = "tutoria@utad.pt",
            //    NormalizedEmail = "TUTORIA@UTAD.PT",
            //    EmailConfirmed = true,
            //    PasswordHash = hasher.HashPassword(null, "Tutoria#utad.pt"),
            //    SecurityStamp = string.Empty
            //};

            //// Seeding default roles
            //modelBuilder.Entity<IdentityRole>().HasData(
            //new IdentityRole { Id = "1", Name = "Manager", NormalizedName = "MANAGER" },
            //new IdentityRole { Id = "2", Name = "Tutor", NormalizedName = "TUTOR" },
            //new IdentityRole { Id = "3", Name = "Mentor", NormalizedName = "MENTOR" },
            //new IdentityRole { Id = "4", Name = "Tutorando", NormalizedName = "TUTORANDO" }
            //);

            //// Add role to default user
            //modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            //    new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = "1" }
            //);
        }
    }
}
