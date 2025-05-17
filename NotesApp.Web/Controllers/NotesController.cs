using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.UseCases.Notes.Commands;
using MediatR;
using NotesApp.Application.UseCases.Notes.Queries;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NotesApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotesController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] CreateNoteCommand command)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await _mediator.Send(command);
        var note = await _mediator.Send(new CreateNoteCommand
        {
            Title = command.Title,
            ContentMarkdown = command.ContentMarkdown,
            CategoryId = command.CategoryId,
        });

        if (note == null)
        {
            return BadRequest("Failed to create note.");
        }

        return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNoteById(Guid id)
    {
        var note = await _mediator.Send(new GetNoteByIdQuery { Id = id });
        if (note == null)
        {
            return NotFound();
        }

        return Ok(note);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notes = await _mediator.Send(new GetAllNotesQuery());
        return Ok(notes);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] UpdateNoteCommand command)
    {
        var note = await _mediator.Send(new UpdateNoteInternalCommand
        {
            Id = id,                            // From URL
            Title = command.Title,              // From body
            Content = command.Content,          // From body
            CategoryId = command.CategoryId     // From body
        });

        if (note == null)
        {
            return NotFound();
        }

        return Ok(note);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(Guid id)
    {
        var result = await _mediator.Send(new DeleteNoteCommand { Id = id });
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}