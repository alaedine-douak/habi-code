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
using HabiCode.Api.Services;
using System.Dynamic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using OpenTelemetry.Trace;
using System.Net.Mime;
using Asp.Versioning;

namespace HabiCode.Api.Controllers;

[ApiController]
// UriApiVersioning
//[Route("v{version:apiVersion}/habits")]
//[ApiVersion(1.0)]
//[ApiVersion(2.0)]

// MediaTypeVersion
[Route("habits")]
[ApiVersion(1.0)]
public sealed class HabitsController(HabiCodeDbContext dbContext, LinkService linkService) : ControllerBase
{
    [HttpGet]
    //[Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.Application.HateoasJson)]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService shapingService)
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

        if (!shapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
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

        // 4.2. pagination
        //var paginationResult = await PaginationResult<HabitDto>.CreateAsync(
        //    habitsQuery,
        //    query.Page,
        //    query.PageSize);

        // 5. data shaping
        
        int totalCount = await habitsQuery.CountAsync();
        List<HabitDto> habits = await habitsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        bool includeLinks = query.Accept == CustomMediaTypeNames.Application.HateoasJson;

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = shapingService.ShapeCollectionData(
                habits,
                query.Fields,
                includeLinks ? h => CreateLinksForHabit(h.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };

        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(
           query,
           paginationResult.HasNextPage,
           paginationResult.HasPreviousPage);
        }

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    //[MapToApiVersion(1.0)] // UriApiVersioning
    [MapToApiVersion(1.0)] // MediaTypeVersion
    public async Task<IActionResult> GetHabit(
        string id, 
        string? fields,
        [FromHeader(Name = "Accept")] string? accept,
        DataShapingService shapingService)
    {
        if (!shapingService.Validate<HabitWithTagsDto>(fields))
        {
            return Problem(
            statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{fields}'");
        }

        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = shapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJson)
        {
            var links = CreateLinksForHabit(id, fields);

            shapedHabitDto.TryAdd("links", links);
        }

        return Ok(shapedHabitDto);
    }

    [HttpGet("{id}")]
    //[MapToApiVersion(2.0)] // UriApiVersioning
    [ApiVersion(2.0)]
    public async Task<IActionResult> GetHabitV2(
        string id,
        string? fields,
        [FromHeader(Name = "Accept")] string? accept,
        DataShapingService shapingService)
    {
        if (!shapingService.Validate<HabitWithTagsDtoV2>(fields))
        {
            return Problem(
            statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{fields}'");
        }

        HabitWithTagsDtoV2? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTagsV2())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = shapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJson)
        {
            var links = CreateLinksForHabit(id, fields);

            shapedHabitDto.TryAdd("links", links);
        }

        return Ok(shapedHabitDto);
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

        habitDto.Links = CreateLinksForHabit(habitDto.Id, null);

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

    private List<LinkDto> CreateLinksForHabits(
        HabitsQueryParameters parameters, 
        bool hasNextPage, 
        bool hasPrevPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            { 
                page =parameters.Page,
                pageSize = parameters.PageSize,
                q = parameters.Search,
                type = parameters.Type,
                sort = parameters.Sort,
                status = parameters.Status,
                fields = parameters.Fields
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post),
        ];


        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                q = parameters.Search,
                type = parameters.Type,
                sort = parameters.Sort,
                status = parameters.Status,
                fields = parameters.Fields
            }));
        }

        if (hasPrevPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "prev-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                q = parameters.Search,
                type = parameters.Type,
                sort = parameters.Sort,
                status = parameters.Status,
                fields = parameters.Fields
            }));
        }

        return links;
    }

    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        return
        [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchHabit), "patch", HttpMethods.Patch, new { id }),
            linkService.Create(nameof(DeleteHabit), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(
                nameof(HabitTagsController.UpsertHabitTags), 
                "upsert-tags", 
                HttpMethods.Put, 
                new { habitId = id },
                HabitTagsController.Name)
        ];
    }
}
