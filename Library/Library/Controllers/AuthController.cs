using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;
using LibraryRestModels;
using Microsoft.AspNetCore.Authorization;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _userService.ValidateLoginAndGenerateTokenAsync(dto.Email, dto.Password);
            if (token == null)
                return Unauthorized("Invalid email or password");

            return (Ok(new { token }));
        }

        [Authorize]
        [HttpGet("secure-data")]
        public IActionResult SecureData()
        {
            return Ok("You are authorized!");
        }

    }
}
