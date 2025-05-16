using Microsoft.AspNetCore.Identity;

namespace HabiCode.Api.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime ExpiresAtUTC { get; set; }

    public IdentityUser User { get; set; }
}
