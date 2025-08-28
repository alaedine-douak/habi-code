using HabiCode.Api.Database;
using HabiCode.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabiCode.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using HabiCodeDbContext dbContext = 
            scope.ServiceProvider.GetRequiredService<HabiCodeDbContext>();
        await using HabiCodeIdentityDbContext identityDbContext = 
            scope.ServiceProvider.GetRequiredService<HabiCodeIdentityDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("HabiCode database migrations applied successfully.");


            await identityDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("HabiCode Identity database migrations applied successfully.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while applying database migrations");
            throw;
        }
    }

    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        RoleManager<IdentityRole> roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            if (!await roleManager.RoleExistsAsync(Roles.Member))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Member));
            }
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }

            app.Logger.LogInformation("Successfully created roles");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex ,"An error occurred while seeding initial data");
            throw;
        }
    }
}
