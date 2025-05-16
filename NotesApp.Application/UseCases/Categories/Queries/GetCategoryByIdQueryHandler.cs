using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.UseCases.Categories.Queries;

public class GetCategoryByIdQuery : IRequest<CategoryWithNotesDto?>
{
    public Guid Id { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryWithNotesDto?>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly INoteRepository _noteRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, INoteRepository noteRepository)
    {
        _categoryRepository = categoryRepository;
        _noteRepository = noteRepository;
    }

    public async Task<CategoryWithNotesDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found.");
        }

        var notes = await _noteRepository.GetNotesByCategoryIdAsync(request.Id, cancellationToken);

        var notesDto = notes.Select(note => new NoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.ContentMarkdown,
            CategoryId = note.CategoryId.ToString() ?? string.Empty,
            CategoryName = note.Category?.Name ?? string.Empty,
            Created = note.Created,
            LastModified = note.LastModified
        }).ToList();

        return new CategoryWithNotesDto
        {
            Id = category.Id,
            Name = category.Name,
            Notes = notesDto ?? [],
        };
    }
}
