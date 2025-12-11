using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using TutoringApp.Data;

namespace TutoringApp.AuthService
{
    public class LoginInfo
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class BlazorCookieLoginMiddleware
    {
        public static IDictionary<Guid, LoginInfo> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginInfo>();


        private readonly RequestDelegate _next;
        private readonly ExternalAuthService _externalAuthService;

        public BlazorCookieLoginMiddleware(
            RequestDelegate next,
            ExternalAuthService externalAuthService
        )
        {
            _next = next;
            _externalAuthService = externalAuthService;

        }

        public async Task Invoke(HttpContext context, SignInManager<ApplicationUser> _signInManager,
            UserManager<ApplicationUser> _userManager)
        {
            if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);
                var info = Logins[key];

                var userInfo = await _externalAuthService.LoginAsync(info.Username, info.Password);

                if(userInfo != null)
                {
                    var user = await _userManager.FindByNameAsync(userInfo.Username);
                    if(user != null)
                    {
                        // Estabelecer sessão
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        info.Password = null;

                        Logins.Remove(key);
                        context.Response.Redirect("./");
                        return;
                    }
                }
                //TODO: Proper error handling
                context.Response.Redirect("/loginfailed");
                return;     
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
