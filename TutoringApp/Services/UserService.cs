using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TutoringApp.Data;
using TutoringApp.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace TutoringApp.Services
{
    public class UserService : IUserService
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private IConfiguration _configuration;
        private string jsonString;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // -------------- CREATE USER ------------------

        public async Task<(bool Success, string Message)> CreateUser(string username)
        {
            var httpClient = CreateHttpClient();

            var studentIupi = await GetStudentIUPIAsync(httpClient, username.Substring(2));
            if (!string.IsNullOrEmpty(studentIupi))
            {
                return await CreateStudentUserAsync(httpClient, studentIupi, username);
            }

            var docenteInfo = await GetDocenteInfoAsync(httpClient, username);
            if (docenteInfo != null)
            {
                return await CreateDocenteUser(docenteInfo, username);
            }

            return (false, "Usuário não encontrado.");
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_configuration["Quovadis_Username"]}:{_configuration["Quovadis_Password"]}")));
            return httpClient;
        }

        private async Task<string> GetStudentIUPIAsync(HttpClient httpClient, string username)
        {
            string studentIUPIUrl = $"{_configuration["WS_GetStudentIUPI"]}{username}";
            var response = await httpClient.GetAsync(studentIUPIUrl);
            if (response.IsSuccessStatusCode)
            {
                string iupi = await response.Content.ReadAsStringAsync();
                return iupi.Trim('\"') != "00000000-0000-0000-0000-000000000000" ? iupi.Trim('\"') : null;
            }
            return null;
        }

        private async Task<(bool Success, string Message)> CreateStudentUserAsync(HttpClient httpClient, string iupi, string username)
        {
            string studentInfoUrl = $"{_configuration["WS_GetStudentByIUPI"]}{iupi}";
            Console.WriteLine($"[DEBUG] Fetching Student Info from: {studentInfoUrl}");
            var response = await httpClient.GetAsync(studentInfoUrl);
            Console.WriteLine($"[DEBUG] Response Status: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content) && content != "[]")
                {
                    var studentInfos = JsonConvert.DeserializeObject<List<PersonInfo>>(content);
                    if (studentInfos.Count > 0)
                    {
                        var studentInfo = studentInfos.First();
                        CreateUserFromPersonInfo(studentInfo, username);
                        await _context.SaveChangesAsync();
                        return (true, "Utilizador criado com sucesso.");
                    }
                }
            }
            return (false, "Informações do estudante não encontradas.");
        }

        private async Task<PersonInfo> GetDocenteInfoAsync(HttpClient httpClient, string username)
        {
            string docenteUrl = $"{_configuration["WS_GetDocente"]}{username}";
            var response = await httpClient.GetAsync(docenteUrl);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PersonInfo>(content);
            }
            return null;
        }

        private async Task<(bool Success, string Message)> CreateDocenteUser(PersonInfo docenteInfo, string username)
        {
            CreateUserFromPersonInfo(docenteInfo, username);
            await _context.SaveChangesAsync();
            return (true, "Docente encontrado e usuário criado.");
        }

        private void CreateUserFromPersonInfo(PersonInfo personInfo, string username)
        {
            string[] nameParts = personInfo.Nome.Split(' ');
            string firstName = CapitalizeName(nameParts.FirstOrDefault());
            string lastName = CapitalizeName(nameParts.LastOrDefault());

            ApplicationUser newUser = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = username,
                NormalizedUserName = username,
                Email = personInfo.Email ?? personInfo.OriginalEmail_Deprecated,
                NormalizedEmail = personInfo.Email ?? personInfo.OriginalEmail_Deprecated,
                NumberMec = personInfo.Numero
            };
            _context.Users.Add(newUser);
        }
        // -------------------------------------------------

        public List<ApplicationUser> GetUsersAdmin()
        {
            // Find ID role "Admin"
            var adminRoleId = _context.Roles
                                            .Where(r => r.Name == "Admin")
                                            .Select(r => r.Id)
                                            .FirstOrDefault();
            // If adminRoleId was null: return empty list. else: find users with "Admin" role
            return adminRoleId == null
                    ? new List<ApplicationUser>()
                    : _context.Users
                                    .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
                                    .ToList();
        }
        public List<ApplicationUser> GetUsersManager()
        {
            // Find ID role "Manager"
            var managerRoleId = _context.Roles
                                            .Where(r => r.Name == "Manager")
                                            .Select(r => r.Id)
                                            .FirstOrDefault();
            // If managerRoleId was null: return empty list. else: find users with "Manager" role
            return managerRoleId == null
                    ? new List<ApplicationUser>()
                    : _context.Users
                                    .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == managerRoleId))
                                    .ToList();
        }
        public async Task AddRoleManager(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            
            if (user == null)
            {
                var httpClient = CreateHttpClient();
                var docenteInfo = await GetDocenteInfoAsync(httpClient, username);
                if (docenteInfo != null)
                {
                    var response = await CreateDocenteUser(docenteInfo, username);
                    user = await _userManager.FindByNameAsync(username);
                    if (!response.Success)
                    {
                        throw new Exception("Utilizador não encontrado.");

                    }
                }
            }

            var managerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");

            // Find association User/Role
            var userRole = await _context.UserRoles
                                         .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == managerRole.Id);

            if (userRole == null)
            {
                // add role
                // O usuário não tem a role, então adicionar a role ao usuário
                userRole = new IdentityUserRole<string> { UserId = user.Id, RoleId = managerRole.Id };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteRoleManager(ApplicationUser user)
        {
            // Find ID role "Manager"
            var managerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
            if (managerRole == null)
            {
                throw new Exception("Role 'Manager' não encontrada.");
            }

            // Find association User/Role
            var userRole = await _context.UserRoles
                                         .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == managerRole.Id);

            if (userRole != null)
            {
                // remove role
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        // Auxiliar classes ---------------------------------------
        public class PersonInfo
        {
            // Name of attributes is the same as api response
            public string? Numero { get; set; }
            public string Nome { get; set; }
            public string? Email { get; set; }
            public string? OriginalEmail_Deprecated { get; set; }
            public string? Username_Deprecated { get; set; }
        }
        private string CapitalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            return char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }
        // ---------------------------------------------------------

    }
}
