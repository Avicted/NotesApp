using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NotesApp.Application.DTOs;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.UseCases.Notes.Commands;

public class CreateNoteCommand : IRequest<NoteDto>
{
    public string Title { get; set; } = string.Empty;
    public string? ContentMarkdown { get; set; }
    public Guid? CategoryId { get; set; }
}


public class CreateNoteHandler : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateNoteHandler(INoteRepository noteRepository, ICategoryRepository categoryRepository, IHttpContextAccessor httpContextAccessor)
    {
        _noteRepository = noteRepository;
        _categoryRepository = categoryRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Invalid CategoryId.");
            }
        }

        if (string.IsNullOrEmpty(request.Title))
        {
            throw new NoteOperationException("Note title cannot be empty.");
        }

        var note = new Note
        {
            Title = request.Title,
            ContentMarkdown = request.ContentMarkdown ?? string.Empty,
            UserId = userId,
            CategoryId = request.CategoryId,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var created = await _noteRepository.CreateNoteAsync(note, cancellationToken);

        return new NoteDto
        {
            Id = created.Id,
            Title = created.Title,
            Content = created.ContentMarkdown,
            CategoryId = created.CategoryId.ToString() ?? string.Empty,
            Created = created.Created,
            LastModified = created.LastModified
        };
    }
}
