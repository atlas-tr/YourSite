using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YourSite.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public IndexModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void OnGet()
        {
            ViewData["url"] = Configuration.GetSection("K12NETPartnerInfo:url").Value;
            ViewData["client_id"] = Configuration.GetSection("K12NETPartnerInfo:client_id").Value;
            ViewData["redirect_uri"] = Configuration.GetSection("K12NETPartnerInfo:redirect_uri").Value;
        }
    }
}