using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.UseCases.Categories.Commands;
using NotesApp.Application.UseCases.Categories.Queries;

namespace NotesApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoriesController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var category = await _mediator.Send(new CreateCategoryInternalCommand
        {
            Name = command.Name,
            UserId = userId,
        });

        if (category == null)
        {
            return BadRequest("Failed to create category.");
        }

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var category = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _mediator.Send(new GetAllCategoriesQuery());
        if (categories == null || categories.Count == 0)
        {
            return NotFound();
        }

        return Ok(categories);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var category = await _mediator.Send(new UpdateCategoryInternalCommand
        {
            Id = id,
            Name = command.Name,
            UserId = userId,
        });

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand { Id = id });
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
