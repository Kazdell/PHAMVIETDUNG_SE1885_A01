using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISystemAccountService _accountService;
        private readonly TokenService _tokenService;

        public AuthController(ISystemAccountService accountService, TokenService tokenService)
        {
            _accountService = accountService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = _accountService.Login(model.Email, model.Password);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // In a real app, save RefreshToken to DB linked to User
            // For this assignment, we might return it and expect client to send it back, 
            // but validating it properly requires storage. 
            // We will simplify by just returning it.

            return Ok(new 
            { 
                AccessToken = accessToken, 
                RefreshToken = refreshToken,
                User = new { user.AccountId, user.AccountName, user.AccountRole }
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenModel model)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            // Verify RefreshToken against DB here if we stored it.
            
            // Generate new token
            // We need to fetch user again to generate token
            var userId = int.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var user = _accountService.GetAccountById(userId);

            if (user == null) return Unauthorized();

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
