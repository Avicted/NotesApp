namespace NotesApp.Domain.Entities;

public class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty; // Foreign key to the User entity
    public User? User { get; set; } = null!; // Navigation property to the User entity
}
