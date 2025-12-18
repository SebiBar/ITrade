using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Route("user"), Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            return Ok(await userService.GetUserAsync());
        }

        [HttpPut]
        public async Task<IActionResult> ChangeUsername([FromQuery] string newUsername)
        {
            await userService.ChangeUsernameAsync(newUsername);
            return Ok();
        }

        [HttpPost("tags"), Authorize(Roles = "Specialist")]
        public async Task<IActionResult> AddProfileTag([FromQuery] int tagId)
        {
            return Ok(await userService.AddProfileTagAsync(tagId));
        }

        [HttpDelete("tags/{tagId:int}"), Authorize(Roles = "Specialist")]
        public async Task<IActionResult> RemoveProfileTag([FromRoute] int tagId)
        {
            await userService.RemoveProfileTagAsync(tagId);
            return Ok();
        }

        [HttpPost("links")]
        public async Task<IActionResult> CreateProfileLink([FromQuery] string url)
        {
            return Ok(await userService.CreateProfileLinkAsync(url));
        }

        [HttpDelete("links/{profileLinkId:int}")]
        public async Task<IActionResult> RemoveProfileLink([FromRoute] int profileLinkId)
        {
            await userService.RemoveProfileLinkAsync(profileLinkId);
            return Ok();
        }

    }
}
