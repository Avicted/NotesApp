using Microsoft.EntityFrameworkCore;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;
using NotesApp.Infrastructure.Persistence;

namespace NotesApp.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _context.Categories.ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // Include the category name in the result
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name,
                Created = c.Created,
                LastModified = c.LastModified,
                UserId = c.UserId,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Category?> UpdateCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await GetCategoryByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return false;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
