using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Route("auth")]
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
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            await authService.ForgotPasswordAsync(email);
            return Ok();
        }

        [HttpPost("resolve-forgot-password")]
        public async Task<IActionResult> ResolveForgotPassword([FromBody] ResolveForgotPasswordRequest resolveForgotPasswordRequest)
        {
            await authService.ResolveForgotPasswordAsync(resolveForgotPasswordRequest);
            return Ok();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromQuery] string newPassword)
        {
            await authService.ChangePasswordAsync(newPassword);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            return Ok(await authService.LoginAsync(loginRequest));
        }

        [HttpPost("refresh-tokens")]
        public async Task<IActionResult> RefreshTokens([FromQuery] string refreshToken)
        {             
            return Ok(await tokenService.RefreshTokensAsync(refreshToken));
        }
    }
}