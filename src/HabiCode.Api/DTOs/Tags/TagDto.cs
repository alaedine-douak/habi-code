namespace HabiCode.Api.DTOs.Tags;

public sealed record TagsCollectionDto
{
    public List<TagDto> Results { get; set; }
}

public sealed record TagDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUTC { get; set; }
    public DateTime? UpdatedAtUTC { get; set; }
}
