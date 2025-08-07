using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace instagramClone.API.Controllers;

public class BaseController : ControllerBase
{
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

}