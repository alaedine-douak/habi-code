using HabiCode.Api.Entities;

namespace HabiCode.Api.DTOs.Habits;

//public sealed record HabitWithTagsDto : HabitDto
//{
//    [JsonProperty(Order = int.MaxValue)]
//    public required string[] Tags { get; init; }
//}

public sealed record HabitWithTagsDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabitType Type { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public required HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public required DateTime CreatedAtUTC { get; init; }
    public DateTime? UpdatedAtUTC { get; init; }
    public DateTime? LastCompletedAtUTC { get; init; }

    public required string[] Tags { get; init; }
}
