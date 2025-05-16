using System.Security.Claims;

namespace HabiCode.Api.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static string GetIdentityId(this ClaimsPrincipal principle)
    {
        string? idenitytId = principle.FindFirstValue(ClaimTypes.NameIdentifier);

        return idenitytId;
    }
}
