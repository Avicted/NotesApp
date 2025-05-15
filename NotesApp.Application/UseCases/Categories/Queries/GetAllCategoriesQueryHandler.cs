using MediatR;
using NotesApp.Application.DTOs;
using NotesApp.Application.Interfaces;

namespace NotesApp.Application.UseCases.Categories.Queries;

public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
{
}

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);
        return categories.Select(category => new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Created = category.Created,
            LastModified = category.LastModified
        }).ToList();
    }
}
