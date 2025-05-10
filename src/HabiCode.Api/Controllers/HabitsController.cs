using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;
using HabiCode.Api.Database;
using HabiCode.Api.DTOs.Habits;
using HabiCode.Api.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using HabiCode.Api.Services.Sorting;

namespace HabiCode.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(HabiCodeDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits(
        [FromQuery] HabitsQueryParameters habitsQuery,
        SortMappingProvider sortMappingProvider)
    {
        habitsQuery.Search ??= habitsQuery.Search?.Trim().ToLower();

        // 1. before
        //IQueryable<Habit> query = dbContext.Habits;

        //if (!string.IsNullOrWhiteSpace(search))
        //{
        //    query = query.Where(h => h.Name.ToLower().Contains(search) 
        //        || h.Description != null && h.Description.ToLower().Contains(search));
        //}

        //List<HabitDto> habits = await query
        //    .Select(HabitQueries.ProjectToDto())
        //    .ToListAsync();

        // 2. after
        //List<HabitDto> habits = await dbContext
        //    .Habits
        //    .Where(h => habitsQuery.Search == null ||
        //                h.Name.ToLower().Contains(habitsQuery.Search) ||
        //                h.Description != null && h.Description.ToLower().Contains(habitsQuery.Search))
        //    .Where(h => habitsQuery.Type == null || h.Type == habitsQuery.Type)
        //    .Where(h => habitsQuery.Status == null || h.Status == habitsQuery.Status)
        //    .Select(HabitQueries.ProjectToDto())
        //    .ToListAsync();

        //Expression<Func<Habit, object>> orderBy = habitsQuery.Sort switch
        //{
        //    "name" => h => h.Name,
        //    "description" => h => h.Description,
        //    "type" => h => h.Type,
        //    "status" => h => h.Type,
        //    _ => h => h.Name
        //};

        // 3. sorting
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(habitsQuery.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The Provided sort parameter isn't valid: '{habitsQuery.Sort}'");
        }

        // 3. sorting
        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        // 3. sorting
        List<HabitDto> habits = await dbContext
            .Habits
            .Where(h => habitsQuery.Search == null ||
                        h.Name.ToLower().Contains(habitsQuery.Search) ||
                        h.Description != null && h.Description.ToLower().Contains(habitsQuery.Search))
            .Where(h => habitsQuery.Type == null || h.Type == habitsQuery.Type)
            .Where(h => habitsQuery.Status == null || h.Status == habitsQuery.Status)
            .ApplySort(habitsQuery.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var habitsCollectionDto = new HabitsCollectionDto
        {
            Results = habits
        };

        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit(string id)
    {
        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        //1. before we added ValidationExceptionHandler Middleware
        //ValidationResult validationResult = await validator.ValidateAsync(createHabitDto);
        //if (!validationResult.IsValid)
        //{ 
        //    return BadRequest(validationResult.ToDictionary());
        //}

        //2. After we add ValidationExceptionHandler Middleware
        await validator.ValidateAndThrowAsync(createHabitDto);

        Habit habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();

        return CreatedAtAction(
            nameof(GetHabit),
            new { id = habitDto.Id },
            habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();

        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUTC = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }
}
