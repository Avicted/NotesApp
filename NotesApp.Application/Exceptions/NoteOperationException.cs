namespace NotesApp.Application.Exceptions;

public class NoteOperationException : Exception
{
    public Guid NoteId { get; }

    public NoteOperationException(Guid noteId, string message)
        : base(message)
    {
        NoteId = noteId;
    }
}
