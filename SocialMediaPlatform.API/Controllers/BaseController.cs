using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SocialMediaPlatform.API.Controllers;

public class BaseController : ControllerBase
{
    protected Guid CurrentUserId => 
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected Guid? CurrentUserIdOrNull
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim) : null;
        }
    }
}