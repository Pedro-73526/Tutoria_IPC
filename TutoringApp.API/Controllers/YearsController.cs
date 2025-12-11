using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using TutoringApp;
using TutoringApp.Models;
using TutoringApp.Services;

namespace TutoringApp.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class YearsController : ControllerBase
    {
        private readonly IYearService _yearService;

        public YearsController(IYearService yearService)
        {
            _yearService = yearService;
        }

        [HttpGet("years")]
        public ActionResult<IEnumerable<Year>> GetYears()
        {
            var years = _yearService.GetYears().Select(y => new { y.YearId, y.YearAcademic, y.IsActive });
  
            return Ok(new { years });
        }

        [HttpGet("year/{yearId}")]
        public ActionResult<object> GetYearInfo(int yearId)
        {
            var yearInfo = _yearService.GetYearInfo(yearId).Result;

            var result = new
            {
                groups = yearInfo.Item1,
                meenteesWithoutGroup = yearInfo.Item2,
                meenteesWithoutMeetings = yearInfo.Item3,
                meetings = yearInfo.Item4,
                mentees = yearInfo.Item5
            };

            return Ok(result);
        }
    }
}
