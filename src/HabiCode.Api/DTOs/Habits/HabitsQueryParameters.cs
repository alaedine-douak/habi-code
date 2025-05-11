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

    // # sorting
    public string? Sort { get; init; }

    // # data shaping
    public string? Fields { get; init; }

    // # pagination
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    // # Content negotiation
    [FromHeader(Name = "Accept")]
    public string? Accept { get; init; }
}
