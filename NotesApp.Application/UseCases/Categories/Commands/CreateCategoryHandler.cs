using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NotesApp.Application.DTOs;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.UseCases.Categories.Commands;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; set; } = string.Empty;
}

public class CreateCategoryInternalCommand : IRequest<CategoryDto>
{
    public string? Name { get; set; }
    public string? UserId { get; set; }
}

public class CreateCategoryHandler : IRequestHandler<CreateCategoryInternalCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateCategoryHandler(ICategoryRepository categoryRepository, IHttpContextAccessor httpContextAccessor)
    {
        _categoryRepository = categoryRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CategoryDto> Handle(CreateCategoryInternalCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        if (string.IsNullOrEmpty(request.Name))
        {
            throw new CategoryOperationException("Category name cannot be empty.");
        }

        var category = new Category
        {
            Name = request.Name,
            UserId = userId,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var created = await _categoryRepository.CreateCategoryAsync(category, cancellationToken);

        return new CategoryDto
        {
            Id = created.Id,
            Name = created.Name,
            Created = created.Created,
            LastModified = created.LastModified
        };
    }
}
