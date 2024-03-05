using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace YourSite.Pages
{
    public class StudentModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public string StudentInfo { get; set; }

        public StudentModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task OnGet()
        {
            using var client = new HttpClient();

            //get bearer token from identityserver4 by sending client_id and client_secret

            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = Configuration.GetSection("K12NETPartnerInfo:client_id").Value,
                ["client_secret"] = Configuration.GetSection("K12NETPartnerInfo:client_secret").Value,
                ["grant_type"] = "client_credentials"
            };

            var requestContent = new FormUrlEncodedContent(tokenRequest);

            var response = await client.PostAsync(Configuration.GetSection("K12NETPartnerInfo:url").Value + "/GWCore.Web/connect/token", requestContent);

            response.EnsureSuccessStatusCode();

            //call api with bearer token to get student information

            var tokenResponse = await response.Content.ReadAsStringAsync();

            var token = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(tokenResponse);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token["access_token"].GetString());

            var k12netID = User.FindFirst("K12NETID")?.Value;


            response = await client.GetAsync(Configuration.GetSection("K12NETPartnerInfo:url").Value + "/INTCore.Web/api/Partner/Students/" + k12netID);

            response.EnsureSuccessStatusCode();

            this.StudentInfo = await response.Content.ReadAsStringAsync();
        }
    }
}
