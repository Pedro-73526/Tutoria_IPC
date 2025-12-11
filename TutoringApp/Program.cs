using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System;
using TutoringApp.Areas.Identity;
using TutoringApp.AuthService;
using TutoringApp.Data;
using TutoringApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Regiter the services
builder.Services.AddScoped<IYearService, YearService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEnrollementService, EnrollementService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Adicionei o `.AddSignInManager<SignInManager<ApplicationUser>>()` na tentativa de criar uma sessao para o idp
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

// RadzenBlazor, use Dialog, Notification, ContextMenu and Tooltip components
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// To get info user logged in
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// Registra o HttpClient
builder.Services.AddHttpClient<ExternalAuthService>();

// HttpClient
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Auto-migration and Seeding for Demo purposes
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Seed Roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "Manager", "Tutor" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Admin User
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = "admin@utad.pt";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Demo",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Test Student/Tutor (matches Mock)
        var testUser = await userManager.FindByNameAsync("al55555");
        if (testUser == null)
        {
            testUser = new ApplicationUser
            {
                UserName = "al55555",
                Email = "al55555@utad.pt",
                FirstName = "Aluno",
                LastName = "Dummy",
                EmailConfirmed = true,
                NumberMec = "55555"
            };
            var result = await userManager.CreateAsync(testUser, "Dummy123!");
            if (result.Succeeded)
            {
                // Assign Tutor role for demo purposes so they can see "My Mentees"
                await userManager.AddToRoleAsync(testUser, "Tutor"); 
            }
        }

        // Seed Active Year
        var activeYear = context.Years.FirstOrDefault(y => y.YearAcademic == "2024/2025");
        if (activeYear == null)
        {
            activeYear = new TutoringApp.Models.Year
            {
                // YearId is Identity, let DB assign it
                YearAcademic = "2024/2025",
                IsActive = true
            };
            context.Years.Add(activeYear);
            await context.SaveChangesAsync();
        }

        // Seed Courses
        var courses = new List<TutoringApp.Models.Course>
        {
            new TutoringApp.Models.Course { CourseCode = 9001, CourseName = "Engenharia Informática", School = "ECT" },
            new TutoringApp.Models.Course { CourseCode = 9002, CourseName = "Engenharia Mecânica", School = "ECT" },
            new TutoringApp.Models.Course { CourseCode = 9003, CourseName = "Medicina Veterinária", School = "ECAV" },
            new TutoringApp.Models.Course { CourseCode = 9004, CourseName = "Biologia", School = "ECVA" }
        };

        foreach (var c in courses)
        {
            if (!context.Courses.Any(dbC => dbC.CourseCode == c.CourseCode))
            {
                Console.WriteLine($"[SEEDING] Adding course: {c.CourseName}");
                context.Courses.Add(c);
            }
            else
            {
                 Console.WriteLine($"[SEEDING] Course already exists: {c.CourseName}");
            }
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"[SEEDING] Courses saved. Total count: {context.Courses.Count()}");


        // Seed Enrollements & Groups for al55555 (Mentor)
        var engInfCourse = context.Courses.FirstOrDefault(c => c.CourseCode == 9001);
        if (engInfCourse != null && testUser != null)
        {
            // 1. Enrollment for Mentor (al55555)
            // Check if already enrolled
            var mentorEnrollment = context.Enrollements.FirstOrDefault(e => e.UserId == testUser.Id && e.YearId == activeYear.YearId);
            if (mentorEnrollment == null)
            {
                mentorEnrollment = new TutoringApp.Models.Enrollement
                {
                    Role = "Mentor",
                    UserId = testUser.Id,
                    YearId = activeYear.YearId,
                    CourseId = engInfCourse.CourseId
                };
                context.Enrollements.Add(mentorEnrollment);
                await context.SaveChangesAsync();
            }

            // 2. Create 5 Students and their Enrollments/Groups
            for (int i = 1; i <= 5; i++)
            {
                string studentUsername = $"aluno{i}";
                var studentUser = await userManager.FindByNameAsync(studentUsername);
                if (studentUser == null)
                {
                    studentUser = new ApplicationUser
                    {
                        UserName = studentUsername,
                        Email = $"{studentUsername}@utad.pt",
                        FirstName = "Aluno",
                        LastName = $"Dummy {i}",
                        EmailConfirmed = true,
                        NumberMec = $"{50000 + i}"
                    };
                    await userManager.CreateAsync(studentUser, "Dummy123!");
                }

                // Check Enrollment
                var studentEnrollment = context.Enrollements.FirstOrDefault(e => e.UserId == studentUser.Id && e.YearId == activeYear.YearId);
                if (studentEnrollment == null)
                {
                    studentEnrollment = new TutoringApp.Models.Enrollement
                    {
                        Role = "Tutorando",
                        UserId = studentUser.Id,
                        YearId = activeYear.YearId,
                        CourseId = engInfCourse.CourseId
                    };
                    context.Enrollements.Add(studentEnrollment);
                    await context.SaveChangesAsync();
                }

                // 3. Create Group (Mentor + Mentee)
                // Check if group exists for this pair
                bool groupExists = context.Groups.Include(g => g.Enrollements)
                                                 .Any(g => g.YearId == activeYear.YearId &&
                                                           g.Enrollements.Any(e => e.Id == mentorEnrollment.Id) &&
                                                           g.Enrollements.Any(e => e.Id == studentEnrollment.Id));
                
                if (!groupExists)
                {
                    var newGroup = new TutoringApp.Models.Group
                    {
                        YearId = activeYear.YearId,
                        Enrollements = new List<TutoringApp.Models.Enrollement> { mentorEnrollment, studentEnrollment }
                    };
                    context.Groups.Add(newGroup);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating/seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<BlazorCookieLoginMiddleware>();
app.UseMiddleware<DisableInProductionMiddleware>();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
