using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("api/dashboard")]
    public class DashboardController(
        IDashboardService dashboardService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            return Ok(await dashboardService.GetDashboardAsync());
        }
    }
}
