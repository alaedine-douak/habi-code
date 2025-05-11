using HabiCode.Api.DTOs.Common;
using HabiCode.Api.DTOs.Habits;

namespace HabiCode.Api.DTOs.Tags;

public sealed record TagDto : ILinksResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUTC { get; set; }
    public DateTime? UpdatedAtUTC { get; set; }
    public List<LinkDto> Links { get; set; }
}
