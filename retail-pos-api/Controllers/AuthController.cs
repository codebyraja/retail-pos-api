using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QSRAPIServices.Models;
using RetailPosRepository.Services.Repository;
using RetailPosToken.Services.Token;

namespace RetailPosAuthController.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[action]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        public readonly IRepository _services;
        public readonly ITokenService _tokenService;
        public AuthController(IRepository services, ITokenService tokenService)
        {
            _services = services;
            _tokenService = tokenService;
        }

        [HttpPost()]
        public async Task<IActionResult> SignIn([FromBody] LoginRequest login)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(login.Username))
                return BadRequest(new { Status = 0, Msg = "Username is required." });

            if (string.IsNullOrWhiteSpace(login.Password))
                return BadRequest(new { Status = 0, Msg = "Password is required." });

            if (!ModelState.IsValid)
                return BadRequest(new { Status = 0, Msg = "Invalid request data." });

            var result = await _services.ValidateUserAsync(login.Username, login.Password);

            if (result.Status != 1 || result.Data == null)
                return Unauthorized(new { Status = 0, Msg = result.Msg });

            var user = result.Data;
            int userId = user.Id;

            // JWT + Refresh Token logic
            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _services.RevokeAllTokensForUser(userId);
            await _services.SaveRefreshToken(userId, refreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new
            {
                Status = 1,
                Msg = "Login successful",
                Token = accessToken,
                RefreshToken = refreshToken,
                Data = user
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest model)
        {
            if (!_tokenService.ValidateRefreshToken(model.RefreshToken))
            {
                return Unauthorized(new { Status = 0, Msg = "Invalid refresh token." });
            }

            var user = await _services.GetUserByRefreshToken(model.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new { Status = 0, Msg = "Refresh token expired or invalid." });
            }

            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _services.UpdateRefreshToken(model.RefreshToken, newRefreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new
            {
                Status = 1,
                Msg = "Token refreshed successfully",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { Status = 0, Msg = "Refresh token is required." });
            }

            var result = await _services.RevokeRefreshToken(request.RefreshToken);

            if (!result)
            {
                return BadRequest(new { Status = 0, Msg = "Invalid or already revoked token." });
            }

            return Ok(new { Status = 1, Msg = "User logged out and token revoked successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupRequest user)
        {
            return Ok(await _services.Signup(user));
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Status = 0, Msg = "Email is required." });
            }

            var result = await _services.SendOtpToEmailAsync(request.Email);

            if (result.Status == 1)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("verify-otp-and-reset")]
        public async Task<IActionResult> VerifyOtpAndReset([FromBody] ResetPasswordRequestWithOtp req)
        {
            var result = await _services.VerifyOtpAndResetPasswordAsync(req.Email, req.Otp, req.NewPassword);
            return Ok(result);
        }
    }
}