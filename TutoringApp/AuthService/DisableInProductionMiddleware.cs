namespace TutoringApp.AuthService
{
    public class DisableInProductionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public DisableInProductionMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (_env.IsProduction())
            {
                var disablePaths = new List<string>
            {
                "/Identity/Account/Login",
                "/Identity/Account/Register"
            };

                if (disablePaths.Contains(context.Request.Path))
                {
                    context.Response.Redirect("/");
                }
            }
            await _next(context);
        }
    }

}
