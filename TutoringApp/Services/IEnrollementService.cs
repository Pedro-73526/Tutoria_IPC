using Microsoft.AspNetCore.Components.Forms;
using TutoringApp.Models;

namespace TutoringApp.Services
{
    public interface IEnrollementService
    {
        Task<List<(string, int)>> GetEnrollementsCount(int yearId);
        Task<(bool Success, string Message, Enrollement? enrollement)> CreateEnrollmentByUsername(string username, int courseId, string role, int yearId);
        Task<(int, int)> CreateEnrollementsByList(List<Enrollement> enrollments, int yearId);
        IEnumerable<Enrollement> GetEnrollements(int yearId);
        IEnumerable<Enrollement> GetEnrollementsByRole(int yearId, string role, int courseId);
        IEnumerable<Enrollement> GetEnrollementsWithoutGroupByRoleAllCourses(int yearId, string role);
        Task DeleteEnrollment(int enrollmentId);
    }
}