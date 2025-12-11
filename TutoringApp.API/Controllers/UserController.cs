using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TutoringApp.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //[HttpPost("login")]
        //public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        //{
        //    var user = await _userManager.FindByNameAsync(loginDto.UserName);

        //    if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
        //    {
        //        var tokenDescriptor = new SecurityTokenDescriptor
        //        {
        //            Subject = new ClaimsIdentity(new Claim[]
        //            {
        //        new Claim("UserID", user.Id.ToString())
        //            }),
        //            Expires = DateTime.UtcNow.AddDays(1),
        //            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"])), SecurityAlgorithms.HmacSha256Signature)
        //        };
        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        //        var token = tokenHandler.WriteToken(securityToken);

        //        return Ok(new { token });
        //    }
        //    else
        //    {
        //        return BadRequest(new { message = "Username or password is incorrect." });
        //    }
        //}

    }
}
