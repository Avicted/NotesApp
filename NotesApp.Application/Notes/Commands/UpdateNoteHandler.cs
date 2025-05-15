using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.Notes.Commands;

public class UpdateNoteCommand
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CategoryId { get; set; } = string.Empty;
}

public class UpdateNoteInternalCommand : IRequest<NoteDto>
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CategoryId { get; set; } = string.Empty;
}

public class UpdateNoteHandler : IRequestHandler<UpdateNoteInternalCommand, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UpdateNoteHandler> _logger;

    public UpdateNoteHandler(INoteRepository noteRepository, IHttpContextAccessor httpContextAccessor, ILogger<UpdateNoteHandler> logger)
    {
        _noteRepository = noteRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _logger.LogInformation("UpdateNoteHandler initialized.");
    }

    public async Task<NoteDto> Handle(UpdateNoteInternalCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var note = await _noteRepository.GetNoteByIdAsync(request.Id, cancellationToken);
        if (note == null)
        {
            throw new NotFoundException($"Note with ID {request.Id} not found.");
        }

        // Check that the note belongs to the user
        if (note.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update a note that does not belong to them. Note ID: {NoteId}", userId, request.Id);
            throw new UnauthorizedAccessException("You do not have permission to update this note.");
        }

        if (!string.IsNullOrEmpty(request.Title))
        {
            note.Title = request.Title;
        }

        if (!string.IsNullOrEmpty(request.Content))
        {
            note.ContentMarkdown = request.Content;
        }

        if (request.CategoryId != null)
        {
            note.CategoryId = !string.IsNullOrEmpty(request.CategoryId)
             ? Guid.Parse(request.CategoryId)
             : null;
        }

        var updatedNote = await _noteRepository.UpdateNoteAsync(note, cancellationToken);
        if (updatedNote == null)
        {
            throw new NoteOperationException(request.Id, "Failed to update the note with ID " + request.Id);
        }

        return new NoteDto
        {
            Id = updatedNote.Id,
            Title = updatedNote.Title,
            Content = updatedNote.ContentMarkdown,
            CategoryId = updatedNote.CategoryId?.ToString() ?? string.Empty,
            Created = updatedNote.Created,
            LastModified = updatedNote.LastModified
        };
    }
}

