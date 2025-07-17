using Microsoft.AspNetCore.Mvc;
using instagramClone.API.Extensions;

namespace instagramClone.API.Controllers;

// Base controller that other controllers can inherit from
// Uses automatic naming but can be overridden by derived classes
[AutoRoute]
public class BaseController : ControllerBase
{
    public BaseController()
    {
        
    }

}