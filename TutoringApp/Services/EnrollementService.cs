using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Radzen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using TutoringApp.Data;
using TutoringApp.Models;

namespace TutoringApp.Services
{
    public class EnrollementService : IEnrollementService
    {
        private ApplicationDbContext _context;
        private readonly IUserService _userService;
        public EnrollementService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public Task<List<(string, int)>> GetEnrollementsCount(int yearId)
        {
            List<(string, int)> countEnrollements = new List<(string, int)>
            {
                { ("Tutores", _context.Enrollements.Count(c => (c.YearId == yearId) && (c.Role == "Tutor"))) },
                { ("Mentores", _context.Enrollements.Count(c => (c.YearId == yearId) && (c.Role == "Mentor"))) },
                { ("Tutorandos", _context.Enrollements.Count(c => (c.YearId == yearId) && (c.Role == "Tutorando"))) }
            };
            return Task.FromResult(countEnrollements);
        }

        public async Task<(bool Success, string Message, Enrollement? enrollement)> CreateEnrollmentByUsername(string username, int courseId, string role, int yearId)
        {
            // se o username nao existir cria user
            if (!await _context.Users.AnyAsync(u => u.UserName == username))
            {
                var (userCreated, userMessage) = await _userService.CreateUser(username);
                if (!userCreated)
                {
                    // if user not found return false and errorMessage
                    return (false, userMessage, null);
                }
            }

            // verifica se enrollment já existe ou cria
            bool enrollmentExists = await _context.Enrollements.AnyAsync(e =>
                e.User.UserName == username &&
                e.Course.CourseId == courseId &&
                e.YearId == yearId &&
                e.Role == role
            );
            if (!enrollmentExists)
            {
                Enrollement newEnrollment = new Enrollement
                {
                    CourseId = courseId,
                    Role = role,
                    YearId = yearId,
                    User = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username),
                };

                if (role == "Tutor")
                {
                    var user = newEnrollment.User;
                    var tutorRoleId = _context.Roles.Single(r => r.Name == "Tutor").Id;
                    if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == tutorRoleId))
                    {
                        await _context.UserRoles.AddAsync(new IdentityUserRole<string> { UserId = user.Id, RoleId = tutorRoleId });
                    }
                }
                await _context.Enrollements.AddAsync(newEnrollment);
                await _context.SaveChangesAsync();
                return (true, "Inscrição efetuada com sucesso.", newEnrollment);

            }
            else
            {
                return (true, "O utilizador já se encontra inscrito.", null);
            }
        }
        public async Task<(int, int)> CreateEnrollementsByList(List<Enrollement> enrollments, int yearId)
        {
            int numEnrollmentAdd = 0;
            int numUsersNotFound = 0;
            foreach (Enrollement enrollment in enrollments)
            {
                // create enrollment
                var (success, _,_) = await CreateEnrollmentByUsername(enrollment.User.UserName, enrollment.Course.CourseId, enrollment.Role, yearId);
                numUsersNotFound += !success ? 1 : 0;
                numEnrollmentAdd += success ? 1 : 0;
            }
            return (numEnrollmentAdd, numUsersNotFound);
        }
        public IEnumerable<Enrollement> GetEnrollements(int yearId)
        {
            return _context.Enrollements
                            .Where(e => e.YearId == yearId)
                            .Include(e => e.Groups)
                            .Include(e => e.User)
                            .Include(e => e.Course)
                            .ToList();
        }
        public IEnumerable<Enrollement> GetEnrollementsByRole(int yearId, string role, int courseId)
        {
            return _context.Enrollements.Include(e => e.Groups)
                                        .Include(e => e.User)
                                        .Include(e => e.Course)
                                        .Where(e => e.YearId == yearId && e.Role == role && e.CourseId == courseId).ToList();
        }
        public IEnumerable<Enrollement> GetEnrollementsWithoutGroupByRoleAllCourses(int yearId, string role)
        {
            var enrollments = _context.Enrollements
                                .Include(e => e.Groups)
                                .Include(e => e.User)
                                .Include(e => e.Course)
                                .Where(e => e.YearId == yearId && e.Role == role)
                                .ToList();

            //return enrollments.Where(e => e.Groups == null || e.Groups.Count == 0);
            var filteredEnrollments = enrollments
                                        .Where(e => e.Groups == null || !e.Groups.Any())
                                        .ToList();

            return filteredEnrollments;
        }
        public async Task DeleteEnrollment(int enrollmentId)
        {
            await _context.Enrollements.Where(e => e.Id == enrollmentId).ExecuteDeleteAsync();
        }
    }
}
