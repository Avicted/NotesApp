using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotesApp.Application.DTOs;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.UseCases.Categories.Commands;

public class UpdateCategoryCommand
{
    public string Name { get; set; } = default!;
}

public class UpdateCategoryInternalCommand : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? UserId { get; set; } = string.Empty;
}

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryInternalCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UpdateCategoryHandler> _logger;

    public UpdateCategoryHandler(ICategoryRepository categoryRepository, IHttpContextAccessor httpContextAccessor, ILogger<UpdateCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _logger.LogInformation("UpdateCategoryHandler initialized.");
    }

    public async Task<CategoryDto> Handle(UpdateCategoryInternalCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found.");
        }

        // Check that the category belongs to the user
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (category.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update a category that does not belong to them. Category ID: {CategoryId}", userId, request.Id);
            _logger.LogWarning("UserId: {UserId}, Category UserId: {CategoryId}", userId, category.UserId);
            throw new UnauthorizedAccessException("You do not have permission to update this category.");
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            category.Name = request.Name;
        }

        var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category, cancellationToken);
        if (updatedCategory == null)
        {
            throw new CategoryOperationException(request.Id, $"Failed to update category with ID {request.Id}.");
        }

        return new CategoryDto
        {
            Id = updatedCategory.Id,
            Name = updatedCategory.Name,
            Created = updatedCategory.Created,
            LastModified = updatedCategory.LastModified
        };
    }
}
