using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.Application.Notes.Commands;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Notes.CommandTests;

public class UpdateNoteHandlerTests
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
    public async Task Handle_ShouldUpdateNote()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var testNotes = new List<Note>(notes);
        var noteToUpdate = testNotes[0];
        var updateNoteCommand = new UpdateNoteInternalCommand
        {
            Id = noteToUpdate.Id,
            Title = "Updated Title",
            Content = "Updated content."
        };

        // Add this setup for the GetNoteByIdAsync call
        mockRepo.Setup(r => r.GetNoteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testNotes.FirstOrDefault(n => n.Id == id));

        mockRepo.Setup(r => r.UpdateNoteAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note note, CancellationToken _) =>
            {
                var existingNote = testNotes.FirstOrDefault(n => n.Id == note.Id);
                if (existingNote != null)
                {
                    existingNote.Title = note.Title;
                    existingNote.ContentMarkdown = note.ContentMarkdown;
                    return existingNote;
                }
                return null;
            });

        var userId = noteToUpdate.UserId; // Use the matching user ID
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateNoteHandler>>();

        var handler = new UpdateNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(updateNoteCommand, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        mockRepo.Verify(r => r.UpdateNoteAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var testNotes = new List<Note>(notes);
        var noteToUpdate = testNotes[0];
        var updateNoteCommand = new UpdateNoteInternalCommand
        {
            Id = noteToUpdate.Id,
            Title = "Updated Title",
            Content = "Updated content."
        };

        // Add this setup for the GetNoteByIdAsync call
        mockRepo.Setup(r => r.GetNoteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testNotes.FirstOrDefault(n => n.Id == id));

        var userId = "unauthorizedUser"; // Use a different user ID
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateNoteHandler>>();

        var handler = new UpdateNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(updateNoteCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenNoteDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var updateNoteCommand = new UpdateNoteInternalCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            Title = "Updated Title",
            Content = "Updated content."
        };

        mockRepo.Setup(r => r.GetNoteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => null);

        var userId = "user123"; // Use a valid user ID
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateNoteHandler>>();

        var handler = new UpdateNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(updateNoteCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNoteOperationException_WhenUpdateFails()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var testNotes = new List<Note>(notes);
        var noteToUpdate = testNotes[0];
        var updateNoteCommand = new UpdateNoteInternalCommand
        {
            Id = noteToUpdate.Id,
            Title = "Updated Title",
            Content = "Updated content."
        };

        // Add this setup for the GetNoteByIdAsync call
        mockRepo.Setup(r => r.GetNoteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testNotes.FirstOrDefault(n => n.Id == id));

        mockRepo.Setup(r => r.UpdateNoteAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note note, CancellationToken _) => null); // Simulate failure

        var userId = noteToUpdate.UserId; // Use the matching user ID
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateNoteHandler>>();

        var handler = new UpdateNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NoteOperationException>(() => handler.Handle(updateNoteCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategoryId_WhenCategoryIdIsProvided()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var noteId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var existingNote = new Note
        {
            Id = noteId,
            UserId = "test-user",
            CategoryId = null
        };

        var command = new UpdateNoteInternalCommand
        {
            Id = noteId,
            CategoryId = newCategoryId.ToString()
        };

        mockRepo.Setup(r => r.GetNoteByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingNote);

        mockRepo.Setup(r => r.UpdateNoteAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note n, CancellationToken _) => n);

        // Act
        var handler = new UpdateNoteHandler(mockRepo.Object, CreateMockHttpContextAccessorWithUser("test-user").Object, new Mock<ILogger<UpdateNoteHandler>>().Object);

        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newCategoryId.ToString(), result.CategoryId);
        mockRepo.Verify(r => r.UpdateNoteAsync(
            It.Is<Note>(n => n.CategoryId == newCategoryId),
            It.IsAny<CancellationToken>()));
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
