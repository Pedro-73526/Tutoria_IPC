using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Radzen;
using System;
using TutoringApp.Models;

namespace TutoringApp.Services
{
    public interface IExcelService
    {
        byte[] CreateTemplate();
        Task<(List<Enrollement> enrollementsToCreate, List<Enrollement> enrollementsDuplicated, List<int> nonExistentCourses)> ReadExcel(IBrowserFile file, int yearId);
        byte[] ExportEnrollmentsTable(IQueryable<Enrollement> enrollments);
        byte[] ExportGroupsTable(IQueryable<CustomGroup> data);
    }
    public class CustomGroup
    {
        public Enrollement Tutor { get; set; }
        public Enrollement Mentor { get; set; }
        public Enrollement Mentee { get; set; }
        public Course Course { get; set; }
        public int NumMeetings { get; set; }
        public int GroupId { get; set; }

        public CustomGroup(Enrollement tutor, Enrollement mentor, Enrollement mentee, Course course, int numMeetings, int groupId)
        {
            Tutor = tutor;
            Mentor = mentor;
            Mentee = mentee;
            Course = course;
            NumMeetings = numMeetings;
            GroupId = groupId;
        }
    }
}
