namespace NotesApp.Application.Exceptions;

public class CategoryOperationException : Exception
{
    public Guid CategoryId { get; }

    public CategoryOperationException(Guid categoryId, string message)
        : base(message)
    {
        CategoryId = categoryId;
    }

    public CategoryOperationException(string? message) : base(message)
    {
    }
}
