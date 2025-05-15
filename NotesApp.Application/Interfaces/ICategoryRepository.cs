using NotesApp.Domain.Entities;

namespace NotesApp.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken);
    Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Category?> UpdateCategoryAsync(Category category, CancellationToken cancellationToken);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
}
