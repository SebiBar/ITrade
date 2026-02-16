using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("projects")]
    public class ProjectController(
        IProjectService projectService,
        ITagService tagService
        ) : ControllerBase
    {
        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> GetProject([FromRoute] int projectId)
        {
            return Ok(await projectService.GetProjectAsync(projectId));
        }

        [HttpPost, Authorize(Roles = "Client")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectRequest projectRequest)
        {
            return Ok(await projectService.CreateProjectAsync(projectRequest));
        }

        [HttpPut("{projectId:int}"), Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateProject(
            [FromRoute] int projectId, [FromBody] ProjectUpdateRequest projectRequest)
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

        [HttpGet("deleted"), Authorize(Roles = "Client,Admin")]
        public async Task<IActionResult> GetDeletedProjects()
        {
            return Ok(await projectService.GetDeletedProjectsAsync());
        }

        [HttpDelete("{projectId:int}/permanent"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> HardDeleteProject([FromRoute] int projectId)
        {
            await projectService.HardDeleteProjectAsync(projectId);
            return Ok();
        }

        [HttpPost("{projectId:int}/restore"), Authorize(Roles = "Client,Admin")]
        public async Task<IActionResult> RestoreProject([FromRoute] int projectId)
        {
            await projectService.RestoreProjectAsync(projectId);
            return Ok();
        }

        [HttpPost("{projectId:int}/tags"), Authorize(Roles = "Client")]
        public async Task<IActionResult> AddProjectTag(
            [FromRoute] int projectId, [FromBody] int tagId)
        {
            return Ok(await tagService.AddProjectTagAsync(projectId, tagId));
        }

        [HttpDelete("{projectId:int}/tags/{tagId:int}"), Authorize(Roles = "Client")]
        public async Task<IActionResult> DeleteProjectTag(
            [FromRoute] int projectId, [FromRoute] int tagId)
        {
            await tagService.RemoveProjectTagAsync(projectId, tagId);
            return Ok();
        }
    }
}