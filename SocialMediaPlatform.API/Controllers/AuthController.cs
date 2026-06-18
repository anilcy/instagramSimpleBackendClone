using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using SocialMediaPlatform.Entities.Dtos.AuthDtos;

namespace SocialMediaPlatform.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
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
        return Ok(authResponse);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResponse = await _authService.LoginAsync(request);
        if (!authResponse.Success)
        {
            return BadRequest(authResponse.Errors);
        }
        return Ok(authResponse);
    }

}
