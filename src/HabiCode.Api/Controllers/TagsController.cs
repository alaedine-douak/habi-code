using System.Net.Mime;
using FluentValidation;
using FluentValidation.Results;
using HabiCode.Api.Database;
using HabiCode.Api.DTOs.Common;
using HabiCode.Api.DTOs.Tags;
using HabiCode.Api.Entities;
using HabiCode.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HabiCode.Api.Controllers;

[Authorize]
[ApiController]
[Route("tags")]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1)]
public sealed class TagsController(
    HabiCodeDbContext dbContext, 
    LinkService linkService,
    UserContext userContext) 
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags([FromHeader] AcceptHeaderDto acceptHeader)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        List<TagDto> tags = await dbContext
            .Tags
            .Where(t => t.UserId == userId)
            .Select(TagQueries.PrjectToDto())
            .ToListAsync();

        var tagsCollectionDto = new TagsCollectionDto
        {
            //1. Implement links for each tach in the collection

            Items = tags
        };

        if (acceptHeader.IncludeLinks)
        {
            tagsCollectionDto.Links = CreateLinksForTags();
        }

        return Ok(tagsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(
        string id, 
        [FromHeader] AcceptHeaderDto acceptHeader)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        TagDto? tag = await dbContext
            .Tags
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(TagQueries.PrjectToDto())
            .FirstOrDefaultAsync();

        if (tag is null)
        {
            return NotFound();
        }

        if (acceptHeader.IncludeLinks)
        {
            tag.Links = CreateLinksForTag(id);
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(
        CreateTagDto createTagDto, 
        IValidator<CreateTagDto> validator,
        ProblemDetailsFactory problemDetailsFactory)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        ValidationResult validationResult =  await validator.ValidateAsync(createTagDto);

        if (!validationResult.IsValid)
        {
            ProblemDetails problem = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest);

            problem.Extensions.Add("errors", validationResult.ToDictionary());

            return BadRequest(problem);
        }

        Tag tag = createTagDto.ToEntity(userId);

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Problem(
                detail: $"The tag '{tag.Name}' already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        dbContext.Tags.Add(tag);

        await dbContext.SaveChangesAsync();

        TagDto tagDto = tag.ToDto();

        tagDto.Links = CreateLinksForTag(tagDto.Id);

        return CreatedAtAction(
            nameof(GetTag),
            new { id = tagDto.Id },
            tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, UpdateTagDto updateTagDto)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (tag is null)
        {
            return NotFound();
        }

        tag.UpdateFromDto(updateTagDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (tag is null)
        {
            return NotFound();
        }

        dbContext.Tags.Remove(tag);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForTags()
    {
        return
        [
            linkService.Create(nameof(GetTags), "self", HttpMethods.Get),
            linkService.Create(nameof(CreateTag), "create", HttpMethods.Post),
        ];
    }

    private List<LinkDto> CreateLinksForTag(string id)
    {
        return
        [
            linkService.Create(nameof(GetTag), "self", HttpMethods.Get, new { id}),
            linkService.Create(nameof(UpdateTag), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(DeleteTag), "delete", HttpMethods.Delete, new { id }),
        ];
    }
}
