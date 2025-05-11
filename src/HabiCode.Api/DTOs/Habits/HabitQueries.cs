using System.Linq.Expressions;
using HabiCode.Api.Entities;

namespace HabiCode.Api.DTOs.Habits;

public static class HabitQueries
{
    public static Expression<Func<Habit, HabitDto>> ProjectToDto()
    {
        return h => new HabitDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Type = h.Type,
            Frequency = new FrequencyDto
            {
                Type = h.Frequency.Type,
                TimesPerPeriod = h.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = h.Target.Value,
                Unit = h.Target.Unit
            },
            Status = h.Status,
            IsArchived = h.IsArchived,
            EndDate = h.EndDate,
            Milestone = h.Milestone == null ? null : new MilestoneDto
            {
                Target = h.Milestone.Target,
                Current = h.Milestone.Current
            },
            CreatedAtUTC = h.CreatedAtUTC,
            UpdatedAtUTC = h.UpdatedAtUTC,
            LastCompletedAtUTC = h.LastCompletedAtUTC
        };
    }


    public static Expression<Func<Habit, HabitWithTagsDto>> ProjectToDtoWithTags()
    {
        return h => new HabitWithTagsDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Type = h.Type,
            Frequency = new FrequencyDto
            {
                Type = h.Frequency.Type,
                TimesPerPeriod = h.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = h.Target.Value,
                Unit = h.Target.Unit
            },
            Status = h.Status,
            IsArchived = h.IsArchived,
            EndDate = h.EndDate,
            Milestone = h.Milestone == null ? null : new MilestoneDto
            {
                Target = h.Milestone.Target,
                Current = h.Milestone.Current
            },
            CreatedAtUTC = h.CreatedAtUTC,
            UpdatedAtUTC = h.UpdatedAtUTC,
            LastCompletedAtUTC = h.LastCompletedAtUTC,
            Tags = h.Tags.Select(t => t.Name).ToArray()
        };
    }


    public static Expression<Func<Habit, HabitWithTagsDtoV2>> ProjectToDtoWithTagsV2()
    {
        return h => new HabitWithTagsDtoV2
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Type = h.Type,
            Frequency = new FrequencyDto
            {
                Type = h.Frequency.Type,
                TimesPerPeriod = h.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = h.Target.Value,
                Unit = h.Target.Unit
            },
            Status = h.Status,
            IsArchived = h.IsArchived,
            EndDate = h.EndDate,
            Milestone = h.Milestone == null ? null : new MilestoneDto
            {
                Target = h.Milestone.Target,
                Current = h.Milestone.Current
            },
            CreatedAt = h.CreatedAtUTC,
            UpdatedAt = h.UpdatedAtUTC,
            LastCompletedAt = h.LastCompletedAtUTC,
            Tags = h.Tags.Select(t => t.Name).ToArray()
        };
    }
}
