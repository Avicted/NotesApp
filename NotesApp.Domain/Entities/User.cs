using Microsoft.AspNetCore.Identity;

namespace NotesApp.Domain.Entities;

public class User : IdentityUser
{
    public ICollection<Note> Notes { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];

    public User()
    {
        Notes = [];
        Categories = [];
    }
}
