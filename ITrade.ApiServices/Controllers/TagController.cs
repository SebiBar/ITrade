using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("tags")]
    public class TagController(ITagService tagService) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<IActionResult> SearchTags([FromQuery] string query)
        {
            return Ok(await tagService.SearchTagsAsync(query));
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTags()
        {
            return Ok(await tagService.GetAllTagsAsync());
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTag([FromBody] string tagName)
        {
            return Ok(await tagService.CreateTagAsync(tagName));
        }

        [HttpDelete("{tagId:int}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTag([FromRoute] int tagId)
        {
            await tagService.DeleteTagAsync(tagId);
            return Ok();
        }
    }
}