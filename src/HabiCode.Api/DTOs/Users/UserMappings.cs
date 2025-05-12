using HabiCode.Api.DTOs.Auth;
using HabiCode.Api.Entities;

namespace HabiCode.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto)
    {
        return new User
        {
            Id = $"u_{Guid.CreateVersion7()}",
            Name = dto.Name,
            Email = dto.Email,
            CreatedAtUTC = DateTime.UtcNow,
        };
    }
}
