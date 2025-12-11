using TutoringApp.Models;

namespace TutoringApp.Services
{
    public interface IYearService
    {
        IEnumerable<Year> GetYears();
        Year? GetYear(int yearId);
        void CreateYear(int year);
        Task DeleteYear(int yearId);
        Task MakeItActive(int yearId);
        Task<Tuple<int, int, int, int, int>> GetYearInfo(int yearId);
    }
}
