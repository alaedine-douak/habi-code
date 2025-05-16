using HabiCode.Api.Database;
using HabiCode.Api.DTOs.Auth;
using HabiCode.Api.DTOs.Users;
using HabiCode.Api.Entities;
using HabiCode.Api.Services;
using HabiCode.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace HabiCode.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController(
    UserManager<IdentityUser> userManager,
    HabiCodeIdentityDbContext identityDbContext,
    HabiCodeDbContext habiCodeDbContext,
    TokenProvider tokenProvider,
    IOptions<JwtAuthOptions> options) 
    : ControllerBase
{

    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

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

        IdentityResult createUserResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);

        if (!createUserResult.Succeeded)
        {
            var extension = new Dictionary<string, object?>
            {
                {
                    "errors",
                    createUserResult.Errors.ToDictionary(e =>  e.Code, e => e.Description)
                }
            };

            return Problem(
                detail: "Unable to register user, please try again!",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extension);
        }

        IdentityResult addToRoleResult = await userManager.AddToRoleAsync(identityUser, Roles.Member);

        if (!addToRoleResult.Succeeded)
        {
            var extension = new Dictionary<string, object?>
            {
                {
                    "errors",
                    addToRoleResult.Errors.ToDictionary(e =>  e.Code, e => e.Description)
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

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email, [Roles.Member]);

        var accessToken = tokenProvider.Create(tokenRequest);

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessToken.RefreshToken,
            ExpiresAtUTC = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays)
        };

        identityDbContext.RefreshTokens.Add(refreshToken);

        await habiCodeDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

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

        IList<string> roles = await userManager.GetRolesAsync(identityUser);

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!, roles);

        AccessTokensDto accessToken = tokenProvider.Create(tokenRequest);

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessToken.RefreshToken,
            ExpiresAtUTC = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays)
        };

        identityDbContext.RefreshTokens.Add(refreshToken);

        await identityDbContext.SaveChangesAsync();

        return Ok(accessToken);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokensDto>> Refresh(RefreshTokenDto refreshTokenDto)
    {
        RefreshToken? refreshToken = await identityDbContext
            .RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

        if (refreshToken is null)
        {
            return Unauthorized();
        }

        if (refreshToken.ExpiresAtUTC < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        IList<string> roles = await userManager.GetRolesAsync(refreshToken.User);

        var tokenRequest = new TokenRequest(refreshToken.User.Id, refreshToken.User.Email!, roles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest); 

        refreshToken.Token = accessTokens.RefreshToken;
        refreshToken.ExpiresAtUTC = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays);

        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }
}
