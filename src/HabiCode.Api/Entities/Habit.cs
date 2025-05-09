namespace HabiCode.Api.Entities;

public sealed class Habit
{
    public string Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public HabitType Type { get; set; }
    public Frequency Frequency { get; set; }
    public Target Target { get; set; }
    public HabitStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly? EndDate { get; set; }
    public Milestone? Milestone { get; set; }
    public DateTime CreatedAtUTC { get; set; }
    public DateTime? UpdatedAtUTC { get; set; }
    public DateTime? LastCompletedAtUTC { get; set; }


    public List<HabitTag> HabitTags { get; set; } // Used for create, update
    public List<Tag> Tags { get; set; } // Used for query 

}

public sealed class Frequency
{
    public FrequencyType Type { get; set; }
    public int TimesPerPeriod { get; set; }
}

public sealed class Target
{
    public int Value { get; set; }
    public string Unit { get; set; }
}

public sealed class Milestone
{
    public int Target { get; set; }
    public int Current { get; set; }
}

public enum HabitStatus
{
    None,
    Ongoing,
    Completed,
}

public enum HabitType
{
    None,
    Binary,
    Measurable
}

public enum FrequencyType
{
    None,
    Daily,
    Weekly,
    Monthly
}
