using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.Notes.Queries;

public class GetAllNotesQuery : IRequest<List<NoteDto>>
{
}


public class GetAllNotesQueryHandler : IRequestHandler<GetAllNotesQuery, List<NoteDto>>
{
    private readonly INoteRepository _noteRepository;

    public GetAllNotesQueryHandler(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<List<NoteDto>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await _noteRepository.GetAllNotesAsync(cancellationToken);
        return notes.Select(note => new NoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.ContentMarkdown,
            Created = note.Created,
            LastModified = note.LastModified
        }).ToList();
    }
}