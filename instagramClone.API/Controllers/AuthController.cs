using System.Security.Claims;
using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace instagramClone.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var authResponse = await _authService.RegisterAsync(request);
        if (!authResponse.Success)
        {
            return BadRequest(authResponse.Errors);
        }
        return Ok(new { token = authResponse.Token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResponse = await _authService.LoginAsync(request);
        if (!authResponse.Success)
        {
            return BadRequest(authResponse.Errors);
        }
        return Ok(new { token = authResponse.Token });
    }
    
    [Authorize]
    [HttpGet("test-auth")]
    public IActionResult TestAuth()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { Message = "Token geçerli!", UserId = userId });
    }

}
