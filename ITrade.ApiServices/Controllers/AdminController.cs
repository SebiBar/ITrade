using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Route("admin"), Authorize(Roles = "Admin")]
    public class AdminController(ITagService tagService) : ControllerBase
    {
        [HttpPost("tags")]
        public async Task<IActionResult> CreateTag([FromQuery] string name)
        {
            return Ok(await tagService.CreateTagAsync(name));
        }

        [HttpDelete("tags/{tagId:int}")]
        public async Task<IActionResult> DeleteTag([FromRoute] int tagId)
        {
            await tagService.DeleteTagAsync(tagId);
            return Ok();
        }

    }
}
