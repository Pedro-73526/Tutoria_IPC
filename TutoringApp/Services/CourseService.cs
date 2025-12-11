using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using TutoringApp.Data;
using TutoringApp.Models;
using TutoringApp.Pages.Manager;
using Newtonsoft.Json;

namespace TutoringApp.Services
{
    public class CourseService : ICourseService
    {
        private ApplicationDbContext _context;
        private IConfiguration _configuration;
        public CourseService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;;
        }

        public async Task CreateCourse(Course course)
        {
            try 
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

            }
            catch
            {
                throw;
            }
        }
        
        public IEnumerable<Course> GetCourses()
        {
            return _context.Courses.ToList();
        }
        public async Task<int> UpdateCourses()
        {
            int coursesAdded = 0;
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_configuration["Quovadis_Username"]}:{_configuration["Quovadis_Password"]}")));

                var response = await httpClient.GetStringAsync(_configuration["WS_GetCourses"]);
                var apiResponses = JsonConvert.DeserializeObject<List<dynamic>>(response);

                var existingCourseCodes = new HashSet<int>(_context.Courses.Select(c => c.CourseCode));

                foreach (var apiResponse in apiResponses)
                {
                    int courseCode = apiResponse.CodigoCurso;
                    string name = apiResponse.Nome;
                    string school = apiResponse.SiglaEscola;

                    if (!existingCourseCodes.Contains(courseCode))
                    {
                        var course = new Course
                        {
                            CourseCode = courseCode,
                            CourseName = name,
                            School = school
                        };
                        _context.Courses.Add(course);
                        coursesAdded++;

                        // Add the new course code to the HashSet to avoid duplicates in the same execution
                        existingCourseCodes.Add(courseCode);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro: " + ex.Message);
            }

            return coursesAdded;
        }
    }
}
