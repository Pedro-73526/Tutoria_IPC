using TutoringApp.Models;

namespace TutoringApp.Services
{
    public interface IGroupService
    {
        Task CreateGroup(List<Enrollement> users, int yearId);
        Task<Enrollement> GetUserByRole(int groupId, string role);
        Task<List<(Enrollement, int, string)>> GetMenteesGroup();
        IEnumerable<Group> GetGroups(int yearId);
        Task<int> CreateGroupsAuto(int courseId, int yearId);
        Task UpdateGroupMembers(int groupId, int tutorId, int mentorId);
        Task DeleteGroup(int groupId);
        Task<bool> IsPermission(int groupId);
        // webservice
        Task<List<(Enrollement, int, string)>> GetGroupsByTutorId(string tutorId);
    }
}
