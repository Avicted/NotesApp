using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;
using NotesApp.Application.Notes.Commands;

namespace NotesApp.Application.Categories.Commands;

public class DeleteCategoryCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DeleteCategoryHandler> _logger;

    public DeleteCategoryHandler(ICategoryRepository categoryRepository, INoteRepository noteRepository, IHttpContextAccessor httpContextAccessor, ILogger<DeleteCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _noteRepository = noteRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _logger.LogInformation("DeleteCategoryCommandHandler initialized.");
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var category = await _categoryRepository.GetCategoryByIdAsync(request.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found.");
        }

        // Check that the category belongs to the user
        if (category.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete a category that does not belong to them. Category ID: {CategoryId}", userId, request.Id);
            throw new UnauthorizedAccessException("You do not have permission to delete this category.");
        }

        // If the category has notes, return error
        var notes = await _noteRepository.GetNotesByCategoryIdAsync(request.Id, cancellationToken);
        if (notes != null && notes.Count > 0)
        {
            _logger.LogWarning("Category with ID {CategoryId} cannot be deleted because it has associated notes.", request.Id);
            throw new InvalidOperationException("Cannot delete a category that has associated notes.");
        }

        return await _categoryRepository.DeleteCategoryAsync(request.Id, cancellationToken);
    }
}
