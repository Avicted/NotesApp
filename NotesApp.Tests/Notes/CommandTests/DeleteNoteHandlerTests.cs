using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.Application.Notes.Commands;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Notes.CommandTests;

public class DeleteNoteHandlerTests
{
    // Setup: Create 2 test notes with different IDs and user IDs that we can test against.
    // Note: In a real-world scenario, you would likely use a test database or in-memory database for this.
    // For simplicity, we are using in-memory objects here.
    private List<Note> notes =
    [
        new Note
        {
            Id = Guid.NewGuid(),
            Title = "Test Note 1",
            ContentMarkdown = "This is a test note.",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            UserId = "user123"
        },
        new Note
        {
            Id = Guid.NewGuid(),
            Title = "Test Note 2",
            ContentMarkdown = "This is another test note.",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            UserId = "user456"
        }
    ];


    [Fact]
    public async Task Handle_ShouldDeleteNote()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var testNotes = new List<Note>(notes); // Use a copy for isolation
        var noteToDelete = testNotes[0];
        var deleteNoteCommand = new DeleteNoteCommand
        {
            Id = noteToDelete.Id,
        };

        mockRepo.Setup(r => r.DeleteNoteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var note = testNotes.FirstOrDefault(n => n.Id == id);
                if (note != null)
                {
                    testNotes.Remove(note);
                    return true;
                }
                return false;
            });

        var userId = noteToDelete.UserId;
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<DeleteNoteHandler>>();

        var handler = new DeleteNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.Handle(deleteNoteCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenNoteDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var deleteNoteCommand = new DeleteNoteCommand
        {
            Id = Guid.NewGuid(),
        };

        mockRepo.Setup(r => r.GetNoteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => null);

        var userId = "user123";
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<DeleteNoteHandler>>();

        var handler = new DeleteNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.Handle(deleteNoteCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotOwnNote()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var deleteNoteCommand = new DeleteNoteCommand
        {
            Id = notes[0].Id,
        };

        mockRepo.Setup(r => r.GetNoteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => notes.FirstOrDefault(n => n.Id == id));

        var userId = "user789"; // Different user ID
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<DeleteNoteHandler>>();

        var handler = new DeleteNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(deleteNoteCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenUserNotAuthenticated()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var deleteNoteCommand = new DeleteNoteCommand
        {
            Id = notes[0].Id,
        };

        mockRepo.Setup(r => r.GetNoteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => notes.FirstOrDefault(n => n.Id == id));

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser("");
        var mockLogger = new Mock<ILogger<DeleteNoteHandler>>();

        var handler = new DeleteNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(deleteNoteCommand, CancellationToken.None));
    }

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessorWithUser(string userId)
    {
        var claims = new List<System.Security.Claims.Claim>
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
            };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        return mockHttpContextAccessor;
    }
}
