using System.Linq.Expressions;
using HabiCode.Api.Entities;

namespace HabiCode.Api.DTOs.Users;

internal static class UserQueries
{
    public static Expression<Func<User, UserDto>> ProjectToDto()
    {
        return u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            CreateAtUTC = u.CreatedAtUTC,
            UpdatedAtUTC = u.UpdatedAtUTC,
        };
    }
}
