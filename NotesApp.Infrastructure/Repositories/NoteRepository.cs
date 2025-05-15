using Microsoft.EntityFrameworkCore;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;
using NotesApp.Infrastructure.Persistence;

namespace NotesApp.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly ApplicationDbContext _context;

    public NoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Note> CreateNoteAsync(Note note, CancellationToken cancellationToken)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);
        return note;
    }

    public async Task<List<Note>> GetAllNotesAsync(CancellationToken cancellationToken)
    {
        return await _context.Notes.ToListAsync(cancellationToken);
    }

    public async Task<Note?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Notes.FindAsync([id], cancellationToken);
    }

    public async Task<Note?> UpdateNoteAsync(Note note, CancellationToken cancellationToken)
    {
        _context.Notes.Update(note);
        await _context.SaveChangesAsync(cancellationToken);
        return note;
    }

    public async Task<bool> DeleteNoteAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await GetNoteByIdAsync(id, cancellationToken);
        if (note == null)
        {
            return false;
        }

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
