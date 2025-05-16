using HabiCode.Api.Database;
using HabiCode.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HabiCode.Api.Services;

public sealed class UserContext(
    IHttpContextAccessor httpContextAccessor,
    HabiCodeDbContext habiCodeDbContext,
    IMemoryCache memoryCache)
{
    private const string CacheKeyPrefix = "users:id";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        string? identityId = httpContextAccessor.HttpContext?.User.GetIdentityId();

        if (identityId is null)
        {
            return null;
        }

        string cackeKey = $"{CacheKeyPrefix}{identityId}";

        string? userId = await memoryCache.GetOrCreateAsync(cackeKey, async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);

            string? userId = await habiCodeDbContext
                .Users
                .Where(u => u.IdentityId == identityId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return userId;
        });

        return userId;
    }
}
