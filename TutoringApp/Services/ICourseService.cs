using TutoringApp.Models;

namespace TutoringApp.Services
{
    public interface ICourseService
    {
        Task CreateCourse(Course course);
        IEnumerable<Course> GetCourses();
        Task<int> UpdateCourses();
    }
}
