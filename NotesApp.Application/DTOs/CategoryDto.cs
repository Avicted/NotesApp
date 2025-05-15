namespace NotesApp.Application.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
}

