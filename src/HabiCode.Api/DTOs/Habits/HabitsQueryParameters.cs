using HabiCode.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HabiCode.Api.DTOs.Habits;

public sealed record HabitsQueryParameters
{
    // # search query
    [FromQuery(Name = "q")]
    public string? Search { get; set; }

    // # filtering
    public HabitType? Type { get; init; }
    public HabitStatus? Status { get; init; }
}
