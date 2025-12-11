using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutoringApp.Services;
using TutoringApp.Models;
using TutoringApp.Pages.Manager;

namespace TutoringApp.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet("groups/{tutorId}")]
        public ActionResult<IEnumerable<Group>> GetGroupsByTutorId()
        {
            var group = _groupService.GetGroupsByTutorId("1");

            return Ok(new { group });
        }
    }
}
