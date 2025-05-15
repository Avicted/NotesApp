using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.UseCases.Notes.Commands;
using MediatR;
using NotesApp.Application.UseCases.Notes.Queries;
using Microsoft.AspNetCore.Authorization;

namespace NotesApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] CreateNoteCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetNoteById), new { id = result.Id }, result);
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