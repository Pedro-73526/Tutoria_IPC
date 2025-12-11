using System.Net.Http.Headers;
using System.Text;

namespace TutoringApp.AuthService
{
    /// <summary>
    /// The ExternalAuthService is designed for external authentication,
    /// particularly with the "quovadis" endpoint of UTAD.  
    /// </summary>
    public class ExternalAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _basicAuthValue;
        private readonly string _authUrl;

        public ExternalAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // Credenciais do endpoint quovadis
            var username = configuration["Quovadis_Username"];
            var password = configuration["Quovadis_Password"];
            _authUrl = configuration["WS_Authentication"];
            _basicAuthValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        }

        /// <summary>
        /// The LoginAsync method verifies the authenticity
        /// of a user's username and password against the "quovadis"
        /// endpoint. It sends the credentials in JSON format and checks
        /// if they match the records on the external service. If the
        /// credentials are valid, it returns a UserInfo object containing
        /// user details like email, username, and IUPI. If the credentials
        /// are invalid, it results in a 401 Unauthorized error.
        /// </summary>
        /// <param name="userUsername">The username of the user.</param>
        /// <param name="userPassword">The password associated with the user's account.</param>
        /// <returns>A UserInfo object containing user details on successful authentication or
        /// null if authentication fails.</returns>
        public async Task<UserInfo> LoginAsync(string userUsername, string userPassword)
        {
            // Configurar o cabeçalho de autenticação Basic Auth com as credenciais estáticas
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _basicAuthValue);


            // Enviar os dados do usuário como JSON
            var response = await _httpClient.PostAsJsonAsync(_authUrl, new { Username = userUsername, Password = userPassword });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInfo>();
            }
            else
            {
                // Tratar erro ou lançar exceção
                return null;
            }
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string Nome { get; set; }
        public string IUPI { get; set; }
    }

}