using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.Notes.Commands;

public class DeleteNoteCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly INoteRepository _noteRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DeleteNoteHandler> _logger;

    public DeleteNoteHandler(INoteRepository noteRepository, IHttpContextAccessor httpContextAccessor, ILogger<DeleteNoteHandler> logger)
    {
        _noteRepository = noteRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _logger.LogInformation("DeleteNoteHandler initialized.");
    }

    public async Task<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var note = await _noteRepository.GetNoteByIdAsync(request.Id, cancellationToken);

        if (note == null)
        {
            throw new NoteOperationException(request.Id, "Failed to delete the note with ID " + request.Id);
        }

        // Check that the note belongs to the user
        if (note.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete a note that does not belong to them. Note ID: {NoteId}", userId, request.Id);
            throw new UnauthorizedAccessException("You do not have permission to delete this note.");
        }

        return await _noteRepository.DeleteNoteAsync(request.Id, cancellationToken);

    }
}
