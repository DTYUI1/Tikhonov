using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WeatherAPI.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    protected string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
    }

    protected string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }
}