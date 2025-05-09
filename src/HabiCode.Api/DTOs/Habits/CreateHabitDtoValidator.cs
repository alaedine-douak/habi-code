using FluentValidation;
using HabiCode.Api.Entities;

namespace HabiCode.Api.DTOs.Habits;

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits =
    [
        "minutes", "hours", "steps", "km", "cal",
        "pages", "books", "tasks", "sessions"
    ];

    private static readonly string[] AllowedUnitsForBinaryHabits = ["sessions", "tasks"];

    public CreateHabitDtoValidator()
    {
        RuleFor(h => h.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Habit name must be between 3 and 100 characters.");

        RuleFor(h => h.Description)
            .MaximumLength(500)
            .When(h => h.Description is not null)
            .WithMessage("Habit description must be less than 200 characters.");

        RuleFor(h => h.Type)
            .IsInEnum()
            .WithMessage("Invalid habit type.");

        RuleFor(h => h.Frequency.Type)
            .IsInEnum()
            .WithMessage("Invalid frequency period.");

        RuleFor(h => h.Frequency.TimesPerPeriod)
            .GreaterThan(0)
            .WithMessage("Frequency times per period must be greater than 0.");

        RuleFor(h => h.Target.Value)
            .GreaterThan(0)
            .WithMessage("Target value must be greater than 0.");

        RuleFor(h => h.Target.Unit)
            .NotEmpty()
            .Must(unit => AllowedUnits.Contains(unit.ToLowerInvariant()))
            .WithMessage($"Unit must be one of: {string.Join(", ", AllowedUnits)}");

        RuleFor(h => h.EndDate)
            .Must(date => date is null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must in the future");

        When(h => h.Milestone is not null, () =>
        {
            RuleFor(x => x.Milestone!.Target)
                .GreaterThan(0)
                .WithMessage("Milestone target must be greater than 0.");
        });

        RuleFor(h => h.Target.Unit)
            .Must((dto, unit) => IsTargetUnitCompatibleWithType(dto.Type, unit))
            .WithMessage("Target unit is not compatible with the habit type");
    }

    private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
    {
        string normalizedUnit = unit.ToLowerInvariant();

        return type switch
        {
            HabitType.Binary => AllowedUnitsForBinaryHabits.Contains(normalizedUnit),
            HabitType.Measurable => AllowedUnits.Contains(normalizedUnit),
            _ => false
        };
    }
}
