namespace NotesApp.Application.DTOs;

public class CategoryWithNotesDto : CategoryDto
{
    public List<NoteDto>? Notes { get; set; } = default!;
}
