using NotesApp.Domain.Entities;

namespace NotesApp.Application.Interfaces;

public interface INoteRepository
{
    Task<Note> CreateNoteAsync(Note note, CancellationToken cancellationToken);
    Task<List<Note>> GetAllNotesAsync(CancellationToken cancellationToken);
    Task<Note?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Note?> UpdateNoteAsync(Note note, CancellationToken cancellationToken);
    Task<bool> DeleteNoteAsync(Guid id, CancellationToken cancellationToken);
}
