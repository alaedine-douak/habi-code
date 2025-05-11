using HabiCode.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace HabiCode.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using HabiCodeDbContext dbContext = 
            scope.ServiceProvider.GetRequiredService<HabiCodeDbContext>();
        await using HabiCodeIdentityDbContext identotyDbContext = 
            scope.ServiceProvider.GetRequiredService<HabiCodeIdentityDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("HabiCode database migrations applied successfully.");


            await identotyDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("HabiCode Identity database migrations applied successfully.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while applying database migrations");
            throw;
        }
    }
}
