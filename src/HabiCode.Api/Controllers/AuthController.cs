using HabiCode.Api.Database;
using HabiCode.Api.DTOs.Auth;
using HabiCode.Api.DTOs.Users;
using HabiCode.Api.Entities;
using HabiCode.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HabiCode.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController(
    UserManager<IdentityUser> userManager,
    HabiCodeIdentityDbContext identityDbContext,
    HabiCodeDbContext habiCodeDbContext,
    TokenProvider tokenProvider) 
    : ControllerBase
{

    [HttpPost("register")]
    public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto registerUserDto)
    {
        using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
        habiCodeDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await habiCodeDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        var identityUser = new IdentityUser
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.Name
        };

        IdentityResult identityResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);

        if (!identityResult.Succeeded)
        {
            var extension = new Dictionary<string, object?>
            {
                {
                    "errors",
                    identityResult.Errors.ToDictionary(e =>  e.Code, e => e.Description)
                }
            };

            return Problem(
                detail: "Unable to register user, please try again!",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extension);
        }

        User user = registerUserDto.ToEntity();
        user.IdentityId = identityUser.Id;

        habiCodeDbContext.Users.Add(user);

        await habiCodeDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email);

        var accessToken = tokenProvider.Create(tokenRequest);

        return Ok(accessToken);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto loginUserDto)
    {
        IdentityUser identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);

        if (identityUser is null || !await userManager.CheckPasswordAsync(identityUser,loginUserDto.Password))
        {
            return Unauthorized();
        }

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!);

        AccessTokensDto accessToken = tokenProvider.Create(tokenRequest);

        return Ok(accessToken);
    }
}
