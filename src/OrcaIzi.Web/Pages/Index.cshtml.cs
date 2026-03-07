using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;

namespace OrcaIzi.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IApiService _apiService;

        public IndexModel(ILogger<IndexModel> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public DashboardDto? DashboardStats { get; set; }
        // public bool IsAuthenticated { get; set; } // Removed as we use User.Identity.IsAuthenticated

        public async Task OnGetAsync()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                DashboardStats = await _apiService.GetDashboardStatsAsync();
            }
        }
    }
}
