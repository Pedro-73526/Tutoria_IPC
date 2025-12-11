using TutoringApp.Data;

namespace TutoringApp.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<(bool Success, string Message)> CreateUser(string username);
        List<ApplicationUser> GetUsersAdmin();
        List<ApplicationUser> GetUsersManager();
        Task AddRoleManager(string username);
        Task DeleteRoleManager(ApplicationUser user);
    }
}
