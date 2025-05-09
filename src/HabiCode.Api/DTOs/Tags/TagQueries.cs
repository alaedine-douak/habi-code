using System.Linq.Expressions;
using HabiCode.Api.Entities;

namespace HabiCode.Api.DTOs.Tags;

public sealed class TagQueries
{
    public static Expression<Func<Tag, TagDto>> PrjectToDto()
    {
        return t => new TagDto 
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            CreatedAtUTC = t.CreateAtUTC,
            UpdatedAtUTC = t.UpdatedAtUTC
        };
    }
}
