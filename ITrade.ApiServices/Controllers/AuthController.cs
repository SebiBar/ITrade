using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Route("api/auth")]
    public class AuthController(
        IAuthService authService,
        ITokenService tokenService
        ) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            await authService.ForgotPasswordAsync(forgotPasswordRequest);
            return Ok();
        }

        [HttpPost("resolve-forgot-password")]
        public async Task<IActionResult> ResolveForgotPassword(
            [FromBody] ResolveForgotPasswordRequest resolveForgotPasswordRequest)
        {
            await authService.ResolveForgotPasswordAsync(resolveForgotPasswordRequest);
            return Ok();
        }

        [HttpPost("change-password"), Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            await authService.ChangePasswordAsync(changePasswordRequest);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            return Ok(await authService.LoginAsync(loginRequest));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] string refreshToken)
        {
            await authService.LogoutAsync(refreshToken);
            return Ok();
        }

        [HttpPost("refresh-tokens")]
        public async Task<IActionResult> RefreshTokens([FromQuery] string refreshToken)
        {             
            return Ok(await tokenService.RefreshTokensAsync(refreshToken));
        }
    }
}