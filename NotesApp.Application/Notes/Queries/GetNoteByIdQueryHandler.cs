using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.Notes.Queries;

public class GetNoteByIdQuery : IRequest<NoteDto>
{
    public Guid Id { get; set; }
}

public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto>
{
    private readonly INoteRepository _noteRepository;

    public GetNoteByIdQueryHandler(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<NoteDto> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetNoteByIdAsync(request.Id, cancellationToken);
        if (note == null)
        {
            throw new NotFoundException($"Note with ID {request.Id} not found.");
        }

        return new NoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.ContentMarkdown,
            Created = note.Created,
            LastModified = note.LastModified
        };
    }
}

