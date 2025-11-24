using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [Route("auth")]
    public class AuthController(IAuthService authService) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            await authService.RegisterAsync(registerRequest);
            return Ok();
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            await authService.VerifyEmailAsync(token);
            return Ok();
        }
    }
}