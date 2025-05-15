using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NotesApp.Application.DTOs;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.UseCases.Notes.Commands;

public class CreateNoteCommand : IRequest<NoteDto>
{
    public string? Title { get; set; }
    public string? ContentMarkdown { get; set; }
}


public class CreateNoteHandler : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateNoteHandler(INoteRepository noteRepository, IHttpContextAccessor httpContextAccessor)
    {
        _noteRepository = noteRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var note = new Note
        {
            Title = request.Title ?? string.Empty,
            ContentMarkdown = request.ContentMarkdown ?? string.Empty,
            UserId = userId,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var created = await _noteRepository.CreateNoteAsync(note, cancellationToken);

        return new NoteDto
        {
            Id = created.Id,
            Title = created.Title,
            Content = created.ContentMarkdown,
            Created = created.Created,
            LastModified = created.LastModified
        };
    }
}
