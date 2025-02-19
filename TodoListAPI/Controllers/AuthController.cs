using Microsoft.AspNetCore.Mvc;
using TodoListAPI.DTOs;
using TodoListAPI.Services;

namespace TodoListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDTO)
        {
            (bool Success, string ErrorMessage, TokenDTO token) result = await _authService.Register(userDTO);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.token);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO userDTO)
        {
            (bool Success, string ErrorMessage, TokenDTO token) result = await _authService.Login(userDTO);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.token);
        }
    }
}
