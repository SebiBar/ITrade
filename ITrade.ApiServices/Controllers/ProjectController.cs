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
            return Ok(await projectService.GetUserProjects());
        }

        [HttpPost, Authorize(Roles = "Client")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectRequestReq projectRequest)
        {
            return Ok(await projectService.CreateProjectAsync(projectRequest));
        }

        [HttpPut("{projectId:int}"), Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateProject(
            [FromRoute] int projectId, [FromBody] ProjectRequestReq projectRequest)
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
