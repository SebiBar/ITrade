using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("projects")]
    public class ProjectController(
        IProjectService projectService
        ) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUserProjects()
        {
            return Ok(await projectService.GetUserProjectsAsync());
        }

        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> GetProject([FromRoute] int projectId)
        {
            return Ok(await projectService.GetProjectAsync(projectId));
        }

        [HttpGet]
        public async Task<IActionResult> SearchProjects([FromQuery] string query)
        {
            return Ok(await projectService.SearchProjectsAsync(query));
        }

        [HttpPost, Authorize(Roles = "Client")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectReq projectRequest)
        {
            return Ok(await projectService.CreateProjectAsync(projectRequest));
        }

        [HttpPut("{projectId:int}"), Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateProject(
            [FromRoute] int projectId, [FromBody] ProjectUpdateReq projectRequest)
        {
            await projectService.UpdateProjectAsync(projectId, projectRequest);
            return Ok();
        }

        [HttpDelete("{projectId:int}"), Authorize(Roles = "Client")]
        public async Task<IActionResult> SoftDeleteProject([FromRoute] int projectId)
        {
            await projectService.SoftDeleteProjectAsync(projectId);
            return Ok();
        }
    }
}
