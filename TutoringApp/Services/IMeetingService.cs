using TutoringApp.Models;

namespace TutoringApp.Services
{
    public interface IMeetingService
    {
        Task CreateMeeting(Meeting meeting);
        Task CreateGroupMeeting(Meeting meeting, List<Enrollement> enrollements);
        IEnumerable<Meeting> GetMeetings(int groupId);
        Task UpdateMeeting(Meeting meeting);
        Task DeleteMeeting(int meetingId);
    }
}
