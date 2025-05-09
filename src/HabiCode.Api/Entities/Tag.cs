namespace HabiCode.Api.Entities;

public sealed class Tag
{
    public string Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreateAtUTC { get; set; }
    public DateTime? UpdatedAtUTC { get; set; }

}
