using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YourSite.Pages
{
    public class Signin_K12NETModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public Dictionary<string, string> UserInfo { get; set; }

        public Signin_K12NETModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task OnGet()
        {
            var code = Request.Query["code"];
            var accessToken = await GetAccessTokenAsync(code);
            var userInfo = await GetUserInformationAsync(accessToken);

            UserInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(userInfo);

            await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("K12NETID", UserInfo["ID"]), new Claim(ClaimTypes.Name, UserInfo["sub"]) }, "Cookies")),
                                                              new AuthenticationProperties
                                                              {
                                                                  IsPersistent = true,
                                                                  ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                                                              });
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            var client = new HttpClient();
            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = Configuration.GetSection("K12NETPartnerInfo:client_id").Value,
                ["client_secret"] = Configuration.GetSection("K12NETPartnerInfo:client_secret").Value,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = Configuration.GetSection("K12NETPartnerInfo:redirect_uri").Value
            };

            var requestContent = new FormUrlEncodedContent(tokenRequest);
            var response = await client.PostAsync(Configuration.GetSection("K12NETPartnerInfo:url").Value + "/GWCore.Web/connect/token", requestContent);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseString);

            return tokenResponse["access_token"].GetString();
        }

        public async Task<string> GetUserInformationAsync(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync(Configuration.GetSection("K12NETPartnerInfo:url").Value + "/GWCore.Web/connect/userinfo");
            response.EnsureSuccessStatusCode();
            var userInfo = await response.Content.ReadAsStringAsync();

            return userInfo;
        }
    }
}
