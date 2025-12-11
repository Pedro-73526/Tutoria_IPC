using Microsoft.EntityFrameworkCore;
using TutoringApp.Data;
using TutoringApp.Models;

namespace TutoringApp.Services
{
    public class MeetingService : IMeetingService
    {
        private ApplicationDbContext _context;
        public MeetingService(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task CreateMeeting(Meeting newMeeting)
        {
            try
            {
                _context.Meetings.Add(newMeeting);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            return Task.CompletedTask;
        }

        public async Task CreateGroupMeeting(Meeting meeting, List<Enrollement> enrollements)
        {
            foreach(var enrollement in enrollements)
            {
                var groupId = enrollement.Groups.Select(g => g.GroupId).FirstOrDefault();
                if (groupId != 0)
                {
                    var newMeeting = new Meeting
                    {
                        Date = meeting.Date,
                        TypeContact = meeting.TypeContact,
                        Subject = meeting.Subject,
                        Description = meeting.Description,
                        GroupId = groupId,
                    };
                    _context.Meetings.Add(newMeeting);
                }
            }
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Meeting> GetMeetings(int groupId)
        {
            try
            {
                var group = _context.Groups.Where(x => x.GroupId == groupId).Include(x=>x.Meetings).FirstOrDefault();
                return group?.Meetings ?? Enumerable.Empty<Meeting>();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task UpdateMeeting(Meeting meeting)
        {
            try
            {
                _context.Meetings.Update(meeting);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            return Task.CompletedTask;
        }

        public async Task DeleteMeeting(int meetingId)
        {
            await _context.Meetings.Where(x => x.Id == meetingId).ExecuteDeleteAsync();
        }

    }
}
