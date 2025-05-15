namespace NotesApp.Application.DTOs;

public class NoteDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string CategoryId { get; set; } = default!;
    public string CategoryName { get; set; } = default!;

    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
}
