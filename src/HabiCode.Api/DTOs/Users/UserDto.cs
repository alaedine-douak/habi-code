namespace HabiCode.Api.DTOs.Users;

public sealed record UserDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required DateTime CreateAtUTC { get; init; }
    public DateTime? UpdatedAtUTC { get; init; }
}
