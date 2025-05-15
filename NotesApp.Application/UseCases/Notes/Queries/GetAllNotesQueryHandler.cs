using MediatR;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.UseCases.Notes.Queries;

public class GetAllNotesQuery : IRequest<List<NoteDto>>
{
}


public class GetAllNotesQueryHandler : IRequestHandler<GetAllNotesQuery, List<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<GetAllNotesQueryHandler> _logger;

    public GetAllNotesQueryHandler(INoteRepository noteRepository, ILogger<GetAllNotesQueryHandler> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
        _logger.LogInformation("GetAllNotesQueryHandler initialized.");
    }

    public async Task<List<NoteDto>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await _noteRepository.GetAllNotesAsync(cancellationToken);

        return notes.Select(note => new NoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.ContentMarkdown,
            CategoryId = note.CategoryId.ToString() ?? string.Empty,
            CategoryName = note.Category?.Name ?? string.Empty,
            Created = note.Created,
            LastModified = note.LastModified
        }).ToList();
    }
}