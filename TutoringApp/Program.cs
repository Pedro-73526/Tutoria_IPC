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
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
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

        // Seed Users and Roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        
        // 1. Roles
        string[] roles = { "Admin", "Manager", "Tutor" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Admin User
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

        // 3. Manager User
        var managerEmail = "manager@utad.pt";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);
        if (managerUser == null)
        {
            managerUser = new ApplicationUser
            {
                UserName = "manager",
                Email = managerEmail,
                FirstName = "Gestor",
                LastName = "Demo",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(managerUser, "Manager123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }

        // 4. Tutor User
        var tutorEmail = "tutor@utad.pt";
        var tutorUser = await userManager.FindByEmailAsync(tutorEmail);
        if (tutorUser == null)
        {
            tutorUser = new ApplicationUser
            {
                UserName = "tutor",
                Email = tutorEmail,
                FirstName = "Tutor",
                LastName = "Demo",
                EmailConfirmed = true,
                NumberMec = "10001"
            };
            var result = await userManager.CreateAsync(tutorUser, "Tutor123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(tutorUser, "Tutor");
            }
        }

        // 5. Mentor User
        var mentorEmail = "mentor@utad.pt";
        var mentorUser = await userManager.FindByEmailAsync(mentorEmail);
        if (mentorUser == null)
        {
            mentorUser = new ApplicationUser
            {
                UserName = "mentor",
                Email = mentorEmail,
                FirstName = "Mentor",
                LastName = "Demo",
                EmailConfirmed = true,
                NumberMec = "10002"
            };
            var result = await userManager.CreateAsync(mentorUser, "Mentor123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(mentorUser, "Tutor");
            }
        }

        // --- NEW SEEDING FOR MANUAL GROUP CREATION TESTING ---
        // Extra Tutor
        var tutor2Email = "tutor2@utad.pt";
        var tutor2User = await userManager.FindByEmailAsync(tutor2Email);
        if (tutor2User == null)
        {
            tutor2User = new ApplicationUser
            {
                UserName = "tutor2",
                Email = tutor2Email,
                FirstName = "Tutor 2",
                LastName = "Extra",
                EmailConfirmed = true,
                NumberMec = "10003"
            };
            var result = await userManager.CreateAsync(tutor2User, "Tutor123!");
            if (result.Succeeded) await userManager.AddToRoleAsync(tutor2User, "Tutor");
        }

        // Extra Mentor
        var mentor2Email = "mentor2@utad.pt";
        var mentor2User = await userManager.FindByEmailAsync(mentor2Email);
        if (mentor2User == null)
        {
            mentor2User = new ApplicationUser
            {
                UserName = "mentor2",
                Email = mentor2Email,
                FirstName = "Mentor 2",
                LastName = "Extra",
                EmailConfirmed = true,
                NumberMec = "10004"
            };
            var result = await userManager.CreateAsync(mentor2User, "Mentor123!");
            if (result.Succeeded) await userManager.AddToRoleAsync(mentor2User, "Tutor");
        }
        // -----------------------------------------------------

        // 6. Tutee User
        var tuteeEmail = "tutee@utad.pt";
        var tuteeUser = await userManager.FindByEmailAsync(tuteeEmail);
        if (tuteeUser == null)
        {
            tuteeUser = new ApplicationUser
            {
                UserName = "tutee",
                Email = tuteeEmail,
                FirstName = "Tutorando",
                LastName = "Demo",
                EmailConfirmed = true,
                NumberMec = "20001"
            };
            await userManager.CreateAsync(tuteeUser, "Tutee123!");
        }

        // 7. Active Year
        var activeYear = context.Years.FirstOrDefault(y => y.YearAcademic == "2024/2025");
        if (activeYear == null)
        {
            activeYear = new TutoringApp.Models.Year
            {
                YearAcademic = "2024/2025",
                IsActive = true
            };
            context.Years.Add(activeYear);
            await context.SaveChangesAsync();
        }

        // 8. Courses
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
                context.Courses.Add(c);
            }
        }
        await context.SaveChangesAsync();

        // 9. Enrollments
        var engInfCourse = context.Courses.FirstOrDefault(c => c.CourseCode == 9001);
        if (engInfCourse != null)
        {
            // Helper to add enrollment if missing
            async Task AddEnrollment(string userId, string roleStr)
            {
                if (!context.Enrollements.Any(e => e.UserId == userId && e.YearId == activeYear.YearId && e.Role == roleStr))
                {
                    context.Enrollements.Add(new TutoringApp.Models.Enrollement
                    {
                        Role = roleStr,
                        UserId = userId,
                        YearId = activeYear.YearId,
                        CourseId = engInfCourse.CourseId
                    });
                }
            }

            if (tutorUser != null) await AddEnrollment(tutorUser.Id, "Tutor");
            if (mentorUser != null) await AddEnrollment(mentorUser.Id, "Mentor");
            if (tuteeUser != null) await AddEnrollment(tuteeUser.Id, "Tutorando");
            // Add enrollments for extras
            if (tutor2User != null) await AddEnrollment(tutor2User.Id, "Tutor");
            if (mentor2User != null) await AddEnrollment(mentor2User.Id, "Mentor");
            
            await context.SaveChangesAsync();
        }

        // 10. Legacy / Bulk Data (al55555 + others)
        // Ensure al55555 is usable as Tutor/Mentor too
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
            var res = await userManager.CreateAsync(testUser, "Dummy123!");
            if (res.Succeeded) await userManager.AddToRoleAsync(testUser, "Tutor");
        }
        
        // Add Legacy Enrollment
        if (testUser != null && engInfCourse != null)
        {
            if (!context.Enrollements.Any(e => e.UserId == testUser.Id && e.YearId == activeYear.YearId))
            {
                 // Add as Mentor for variety
                 context.Enrollements.Add(new TutoringApp.Models.Enrollement
                 {
                     Role = "Mentor",
                     UserId = testUser.Id,
                     YearId = activeYear.YearId,
                     CourseId = engInfCourse.CourseId
                 });
                 await context.SaveChangesAsync();
            }
        }

        // Bulk Tutees
        for (int i = 1; i <= 5; i++)
        {
            string sName = $"aluno{i}";
            var sUser = await userManager.FindByNameAsync(sName);
            if (sUser == null)
            {
                sUser = new ApplicationUser
                {
                    UserName = sName,
                    Email = $"{sName}@utad.pt",
                    FirstName = "Aluno",
                    LastName = $"Dummy {i}",
                    EmailConfirmed = true,
                    NumberMec = $"{50000 + i}"
                };
                await userManager.CreateAsync(sUser, "Dummy123!");
            }
            if (engInfCourse != null && sUser != null && !context.Enrollements.Any(e => e.UserId == sUser.Id && e.YearId == activeYear.YearId))
            {
                 context.Enrollements.Add(new TutoringApp.Models.Enrollement
                 {
                     Role = "Tutorando",
                     UserId = sUser.Id,
                     YearId = activeYear.YearId,
                     CourseId = engInfCourse.CourseId
                 });
            }
        }
        await context.SaveChangesAsync();

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
