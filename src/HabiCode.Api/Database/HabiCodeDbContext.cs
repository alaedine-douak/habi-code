using HabiCode.Api.Database.Configurations;
using HabiCode.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabiCode.Api.Database;

public sealed class HabiCodeDbContext(DbContextOptions<HabiCodeDbContext> options) 
    : DbContext(options)
{

    public DbSet<Habit> Habits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.HabiCode);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HabiCodeDbContext).Assembly);
    }
}
