namespace NotesApp.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty; // Foreign key to the User entity
    public User? User { get; set; } = null!; // Navigation property to the User entity
    public ICollection<Note> Notes { get; set; } = []; // Navigation property to the Note entity
}
