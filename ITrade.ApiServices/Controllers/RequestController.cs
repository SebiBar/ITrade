using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("api/requests")]
    public class RequestController(IRequestService requestService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUserRequests()
        {
            return Ok(await requestService.GetUserRequestsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] RequestReq projectRequest)
        {
            return Ok(await requestService.CreateRequestAsync(projectRequest));
        }

        [HttpPost("{requestId:int}")]
        public async Task<IActionResult> ResolveRequest(
            [FromRoute] int requestId, [FromQuery] bool accepted)
        {
            await requestService.ResolveRequestAsync(requestId, accepted);
            return Ok();
        }

        [HttpDelete("{requestId:int}")]
        public async Task<IActionResult> DeleteRequest([FromRoute] int requestId)
        {
            await requestService.DeleteRequestAsync(requestId);
            return Ok();
        }
    }
}
