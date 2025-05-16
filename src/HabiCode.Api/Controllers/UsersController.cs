using System.Security.Claims;
using HabiCode.Api.Database;
using HabiCode.Api.DTOs.Users;
using HabiCode.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabiCode.Api.Controllers;

[Authorize]
[ApiController] 
[Route("users")]
public sealed class UsersController(
    HabiCodeDbContext dbContext) 
    : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        UserDto? user = await dbContext
            .Users
            .Where(u => u.Id == id)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        string? identityUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (identityUser is null)
        {
            return Unauthorized();
        }

        UserDto? user = await dbContext
            .Users
            .Where(u =>  u.IdentityId ==  identityUser)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
