using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("api/user")]
    public class UserController(
        IUserService userService,
        ITagService tagService) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            return Ok(await userService.GetCurrentUserProfileAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] int id)
        {
            return Ok(await userService.GetUserProfileAsync(id));
        }

        [HttpPut]
        public async Task<IActionResult> ChangeUsername([FromQuery] string newUsername)
        {
            await userService.ChangeUsernameAsync(newUsername);
            return Ok();
        }

        [HttpPost("tags"), Authorize(Roles = "Specialist,Admin")]
        public async Task<IActionResult> AddProfileTag([FromQuery] int tagId)
        {
            return Ok(await tagService.AddProfileTagAsync(tagId));
        }

        [HttpDelete("tags/{tagId:int}"), Authorize(Roles = "Specialist,Admin")]
        public async Task<IActionResult> RemoveProfileTag([FromRoute] int tagId)
        {
            await tagService.RemoveProfileTagAsync(tagId);
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

        [HttpDelete("me")]
        public async Task<IActionResult> SoftDeleteAccount()
        {
            await userService.SoftDeleteAccountAsync();
            return Ok();
        }

        [HttpPost("{userId:int}/restore"), Authorize(Roles="Admin")]
        public async Task<IActionResult> RestoreUser([FromRoute] int userId)
        {
            await userService.RestoreUserAsync(userId);
            return Ok();
        }


        [HttpDelete("{userId:int}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> HardDeleteUser([FromRoute] int userId)
        {
            await userService.HardDeleteUserAsync(userId);
            return Ok();
        }

    }
}
