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
using HabiCode.Api.DTOs.Common;

namespace HabiCode.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(HabiCodeDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginationResult<HabitDto>>> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider)
    {
        query.Search ??= query.Search?.Trim().ToLower();

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
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The Provided sort parameter isn't valid: '{query.Sort}'");
        }

        // 3. sorting
        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        // 3. sorting
        //List<HabitDto> habits = await dbContext
        //    .Habits
        //    .Where(h => query.Search == null ||
        //                h.Name.ToLower().Contains(query.Search) ||
        //                h.Description != null && h.Description.ToLower().Contains(query.Search))
        //    .Where(h => query.Type == null || h.Type == query.Type)
        //    .Where(h => query.Status == null || h.Status == query.Status)
        //    .ApplySort(query.Sort, sortMappings)
        //    .Select(HabitQueries.ProjectToDto())
        //    .ToListAsync();

        // 4. pagination
        //IQueryable<Habit> habitsQuery = dbContext
        //    .Habits
        //    .Where(h => query.Search == null ||
        //            h.Name.ToLower().Contains(query.Search) ||
        //            h.Description != null && h.Description.ToLower().Contains(query.Search))
        //    .Where(h => query.Type == null || h.Type == query.Type)
        //    .Where(h => query.Status == null || h.Status == query.Status);

        //int totalCount = await habitsQuery.CountAsync();

        //List<HabitDto> habits = await habitsQuery
        //    .ApplySort(query.Sort, sortMappings)
        //    .Skip((query.Page - 1) * query.PageSize)
        //    .Take(query.PageSize)
        //    .Select(HabitQueries.ProjectToDto())
        //    .ToListAsync();

        // 4.1. pagination

        IQueryable<HabitDto> habitsQuery = dbContext
            .Habits
            .Where(h => query.Search == null ||
                        h.Name.ToLower().Contains(query.Search) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            .ApplySort(query.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto());            

        var paginationResult = await PaginationResult<HabitDto>.CreateAsync(
            habitsQuery,
            query.Page,
            query.PageSize);

        return Ok(paginationResult);
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
