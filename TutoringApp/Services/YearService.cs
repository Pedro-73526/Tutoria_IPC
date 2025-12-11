using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TutoringApp.Data;
using TutoringApp.Models;
using TutoringApp.Pages.Manager;

namespace TutoringApp.Services
{
    public class YearService : IYearService
    {
        private ApplicationDbContext _context;
        public YearService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Year> GetYears()
        {
            try
            {
                return _context.Years.ToList();
            }
            catch 
            {
                throw;
            }
        }

        public Year? GetYear(int yearId)
        {
            try
            {
                return _context.Years.FirstOrDefault(y => y.YearId == yearId);
            }
            catch 
            {
                throw;
            }
        }

        public void CreateYear(int yearId)
        {
            _context.Database.OpenConnection();
            try 
            {
                Year year = new Year();
                year.YearId = yearId;
                year.YearAcademic = yearId.ToString() + "/" + (yearId + 1).ToString();
                year.IsActive = false;

                _context.Database.ExecuteSqlInterpolated($"SET IDENTITY_INSERT Years ON;");
                _context.Years.Add(year);
                _context.SaveChanges();
                _context.Database.ExecuteSqlInterpolated($"SET IDENTITY_INSERT Years OFF;");
            }
            catch
            {
                throw;
            }
            
        }

        public async Task DeleteYear(int yearId)
        {
            await _context.Years.Where(x => x.YearId == yearId).ExecuteDeleteAsync();
        }
        
        public Task MakeItActive(int yearId)
        {
            foreach (var year in _context.Years.Where(x => x.IsActive == true))
            {
                year.IsActive = false;
            }
            foreach(var year in _context.Years.Where(x=> x.YearId == yearId))
            {
                year.IsActive = true;
            }
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        /// <summary>
        /// This code searches data for one year.
        /// </summary>
        /// <param name="yearId">year of the data to obtain</param>
        /// <returns>Number of groups, Mentees without group</returns>
        public Task<Tuple<int,int, int, int, int>> GetYearInfo(int yearId)
        {
            var enrollmentsCount = _context.Enrollements.Where(e => e.YearId == yearId).Count();
            var groupCount = _context.Groups.Where(g => g.YearId == yearId).Count();
            var menteesWithoutGroup = _context.Enrollements.Where(e => e.YearId == yearId && e.Role == "Tutorando" && e.Groups.Count < 1).Count();
            var meetingCount = _context.Meetings.Where(m => m.Group.YearId == yearId).Count();
            var menteesWithoutMeentings = _context.Groups.Where(g => g.YearId == yearId && g.Meetings.Count < 1).Count();
            return Task.FromResult(Tuple.Create(enrollmentsCount, groupCount, menteesWithoutGroup, meetingCount, menteesWithoutMeentings));
        }
    }
}
