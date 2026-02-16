using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("discovery")]
    public class DiscoveryController(
        IMatchingService matchingService,
        ISearchingService searchingService): ControllerBase
    {
        [HttpGet(), Authorize(Roles = "Specialist")]
        public async Task<IActionResult> GetRecommendedProjectsForSpecialist()
        {
            var recommendedProjects = await matchingService.RecommandProjectsForSpecialist();
            return Ok(recommendedProjects);
        }

        [HttpGet("projects/{projectId:int}"), Authorize(Roles = "Client")]
        public async Task<IActionResult> GetRecommendedSpecialistsForClient([FromRoute] int projectId)
        {
            return Ok(await matchingService.RecommandSpecialistsForProject(projectId));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchRequest request)
        {
            return Ok(await searchingService.SearchAsync(request));
        }
    }
}
