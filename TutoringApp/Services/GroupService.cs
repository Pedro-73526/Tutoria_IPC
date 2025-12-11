using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TutoringApp.Data;
using TutoringApp.Models;
using TutoringApp.Pages.Manager;
using TutoringApp.Pages.Tutor;

namespace TutoringApp.Services
{
    public class GroupService : IGroupService
    {
        private ApplicationDbContext _context;
        private AuthenticationStateProvider _authStateProvider;
        public GroupService(ApplicationDbContext context, AuthenticationStateProvider authStateProvider)
        {
            _context = context;
            _authStateProvider = authStateProvider;
        }

        public async Task CreateGroup(List<Enrollement> users, int yearId)
        {
            var group = new Group { YearId = yearId, Enrollements = users };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
        }

        public async Task<Enrollement> GetUserByRole(int groupId, string role)
        {
            try
            {
                var user = _context.Groups
                    .Where(x => x.GroupId == groupId)
                    .Include(x => x.Enrollements)
                        .ThenInclude(y => y.Course)
                    .Include(x => x.Enrollements)
                        .ThenInclude(y => y.User)
                    .FirstOrDefault()
                    .Enrollements
                    .FirstOrDefault(x => x.Role == role);

                return user;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<(Enrollement, int, string)>> GetMenteesGroup()
        {
            // Get current userId
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var ActiveYear = _context.Years.FirstOrDefault(x => x.IsActive);
            if (ActiveYear == null) return new List<(Enrollement, int, string)>();

            var groups = _context.Groups
                .Include(g => g.Enrollements)
                    .ThenInclude(e => e.Course)
                .Include(g => g.Enrollements)
                    .ThenInclude(e => e.User)
                .Include(g => g.Meetings)
                .Where(g => g.Enrollements.Any(e => e.User.Id == userId && e.YearId == ActiveYear.YearId && e.Role == "Tutor"))
                .ToList();

            var myMentees = groups.SelectMany(g => g.Enrollements
                    .Where(e => e.Role == "Tutorando")
                    .Select(e => (e, g.GroupId, g.Meetings.Any() ? g.Meetings.Max(m => m.Date).ToShortDateString() : "Sem reuniões")))
                .ToList();

            return myMentees;
        }

        /// <summary>
        /// Get all groups from a year
        /// </summary>
        /// <param name="yearId"></param>
        /// <returns></returns>
        public IEnumerable<Group> GetGroups(int yearId)
        {
            return _context.Groups.Where(g => g.YearId == yearId)
                                  .Include(g => g.Enrollements)
                                    .ThenInclude(ge => ge.User)
                                  .Include(g => g.Enrollements)
                                    .ThenInclude(ge => ge.Course)
                                  .Include(g => g.Meetings).ToList();
        }

        /// <summary>
        /// Creates groups for a particular course.
        /// Checks the registered students that don't have
        /// a group and assigns tutors and mentors according to the vacancies
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="yearId"></param>
        /// <returns></returns>
        public async Task<int> CreateGroupsAuto(int courseId, int yearId)
        {
            int numberGroupsCreated = 0;

            var menteesWithoutGroup = _context.Enrollements
                .Where(e => e.CourseId == courseId && e.YearId == yearId && e.Role == "Tutorando" && e.Groups.Count < 1)
                .ToList();

            var tutorsAvailable = _context.Enrollements
                .Where(e => e.CourseId == courseId && e.YearId == yearId && e.Role == "Tutor")
                .ToList();

            var mentorsAvailable = _context.Enrollements
                .Where(e => e.CourseId == courseId && e.YearId == yearId && e.Role == "Mentor")
                .ToList();

            foreach (var mentee in menteesWithoutGroup)
            {
                List<Enrollement> enrollments = new List<Enrollement> { mentee };

                // Ordenar as listas em cada iteração para que os tutores e mentores com menos grupos sejam selecionados primeiro
                tutorsAvailable = tutorsAvailable.OrderBy(e => e.Groups?.Count ?? 0).ToList();
                mentorsAvailable = mentorsAvailable.OrderBy(e => e.Groups?.Count ?? 0).ToList();

                if (tutorsAvailable.Any())
                {
                    var tutor = tutorsAvailable.First();
                    enrollments.Add(tutor);
                    tutor.Groups = tutor.Groups ?? new List<Group>(); // Make sure Groups is not null
                    // tutor.Groups.Add(new Group()); // Increasing the group counter
                }
                else
                {
                    throw new Exception("Não existem tutores disponíveis.");
                }

                if (mentorsAvailable.Any())
                {
                    var mentor = mentorsAvailable.First();
                    enrollments.Add(mentor);
                    mentor.Groups = mentor.Groups ?? new List<Group>(); // Make sure Groups is not null
                    // mentor.Groups.Add(new Group()); // Increasing the group counter
                }

                var group = new Group { YearId = yearId, Enrollements = enrollments };
                _context.Groups.Add(group);
                numberGroupsCreated++;
            }

            await _context.SaveChangesAsync();
            return numberGroupsCreated;
        }

        /// <summary>
        /// This method receives as parameters the Id of the group to which 
        /// we want to update the tutor and/or mentor, as well as the respective registration ids of the tutor and/or mentor.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="tutorId"></param>
        /// <param name="mentorId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateGroupMembers(int groupId, int tutorId, int mentorId)
        {
            var group = await _context.Groups.Include(g => g.Enrollements).FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                throw new Exception("Grupo não encontrado");
            }

            if (tutorId != 0)
            {
                await UpdateEnrollmentRole(group, "Tutor", tutorId);
            }

            if (mentorId != 0)
            {
                await UpdateEnrollmentRole(group, "Mentor", mentorId);
            }

            await _context.SaveChangesAsync();
        }
        private async Task UpdateEnrollmentRole(Group group, string role, int newEnrollmentId)
        {
            var existingEnrollment = group.Enrollements.FirstOrDefault(e => e.Role == role);

            if (existingEnrollment != null)
            {
                // Remove existing Enrollment
                group.Enrollements.Remove(existingEnrollment);
            }

            // Find the new enrollment on DB
            var newEnrollment = await _context.Enrollements.FindAsync(newEnrollmentId);

            if (newEnrollment == null)
            {
                throw new Exception($"Novo Enrollment de {role} não encontrado");
            }

            // Add new enrollment to the group
            group.Enrollements.Add(newEnrollment);
        }
        public async Task DeleteGroup(int groupId)
        {
            await _context.Groups.Where(g=>g.GroupId == groupId).ExecuteDeleteAsync();
        }

        public async Task<bool> IsPermission(int groupId)
        {
            // Get current userId
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var enrollment = await _context.Enrollements
                .Include(e => e.Groups)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Groups.Any(g => g.GroupId == groupId));

            // Retorna verdadeiro se o "enrollment" for encontrado, e falso caso contrário
            return enrollment != null;
        }

        public async Task<List<(Enrollement, int, string)>> GetGroupsByTutorId(string tutorId)
        {
            var ActiveYear = _context.Years.FirstOrDefault(x => x.IsActive);
            if (ActiveYear == null) return new List<(Enrollement, int, string)>();

            var userEnrollments = _context.Enrollements
                .Where(x => x.User.Id == tutorId && x.YearId == ActiveYear.YearId)
                .Include(e => e.Groups)
                .ToList();

            var myMentees = new List<(Enrollement, int, string)>();
            foreach (var userEnrollment in userEnrollments)
            {
                var groups = _context.Groups
                    .Include(g => g.Enrollements)
                        .ThenInclude(e => e.Course)
                    .Include(g => g.Enrollements)
                        .ThenInclude(e => e.User)
                    .Include(g => g.Meetings)
                    .Where(x => x.Enrollements.Any(e => e.Id == userEnrollment.Id));

                foreach (var group in groups)
                {
                    var mentee = group.Enrollements.FirstOrDefault(e => e.Role == "Tutorando");
                    var lastMeeting = group.Meetings.Any() ?
                        group.Meetings.Max(m => m.Date).ToShortDateString() :
                        "Sem reuniões";

                    if (mentee != null)
                    {
                        myMentees.Add((mentee, group.GroupId, lastMeeting));
                    }
                }
            }
            return myMentees;
        }
    }
}