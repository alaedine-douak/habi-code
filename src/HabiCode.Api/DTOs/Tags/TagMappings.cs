using HabiCode.Api.Entities;

namespace HabiCode.Api.DTOs.Tags;

public static class TagMappings
{
    public static Tag ToEntity(this CreateTagDto dto, string userId)
    {
        Tag tag = new()
        {
            Id = $"t_{Guid.CreateVersion7()}",
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            CreateAtUTC = DateTime.UtcNow
        };

        return tag;
    }

    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUTC = tag.CreateAtUTC,
            UpdatedAtUTC = tag.UpdatedAtUTC
        };
    }

    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        tag.UpdatedAtUTC = DateTime.UtcNow;
    }
}
