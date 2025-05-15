using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.Application.DTOs;
using NotesApp.Application.Interfaces;
using NotesApp.Application.Notes.Queries;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Notes.QueryTests;

public class GetNotesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotesForUser()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        mockRepo.Setup(r => r.GetAllNotesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Note>
                {
                    new Note
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test",
                        ContentMarkdown = "Test content",
                        Created = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow,
                        UserId = "user123"
                    }
                });

        var mockLogger = new Mock<ILogger<GetAllNotesQueryHandler>>();
        var handler = new GetAllNotesQueryHandler(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllNotesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        Assert.Equal("Test", result.First().Title);
        Assert.Equal("Test content", result.First().Content);
        Assert.IsType<List<NoteDto>>(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoNotes()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        mockRepo.Setup(r => r.GetAllNotesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Note>());

        var mockLogger = new Mock<ILogger<GetAllNotesQueryHandler>>();
        var handler = new GetAllNotesQueryHandler(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllNotesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        mockRepo.Setup(r => r.GetAllNotesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

        var mockLogger = new Mock<ILogger<GetAllNotesQueryHandler>>();
        var handler = new GetAllNotesQueryHandler(mockRepo.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(new GetAllNotesQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnNotesWithCorrectProperties()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var noteId = Guid.NewGuid();
        var noteTitle = "Test Note";
        var noteContent = "This is a test note.";
        var noteCreated = DateTime.UtcNow;
        var noteLastModified = DateTime.UtcNow;
        var userId = "user123";

        mockRepo.Setup(r => r.GetAllNotesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Note>
                {
                    new Note
                    {
                        Id = noteId,
                        Title = noteTitle,
                        ContentMarkdown = noteContent,
                        Created = noteCreated,
                        LastModified = noteLastModified,
                        UserId = userId
                    }
                });

        var mockLogger = new Mock<ILogger<GetAllNotesQueryHandler>>();
        var handler = new GetAllNotesQueryHandler(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllNotesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Single(result);

        var noteDto = result.First();

        Assert.Equal(noteId, noteDto.Id);
        Assert.Equal(noteTitle, noteDto.Title);
        Assert.Equal(noteContent, noteDto.Content);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotesInCorrectOrder()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var note1 = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note 1",
            ContentMarkdown = "Content 1",
            Created = DateTime.UtcNow.AddDays(-1),
            LastModified = DateTime.UtcNow.AddDays(-1),
            UserId = "user123"
        };
        var note2 = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note 2",
            ContentMarkdown = "Content 2",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            UserId = "user123"
        };

        mockRepo.Setup(r => r.GetAllNotesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Note> { note2, note1 });

        var mockLogger = new Mock<ILogger<GetAllNotesQueryHandler>>();
        var handler = new GetAllNotesQueryHandler(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllNotesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Note 2", result[0].Title);
        Assert.Equal("Note 1", result[1].Title);
    }

    [Fact]
    public async Task Handle_ShouldReturnNoteWithCorrectId()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var noteId = Guid.NewGuid();
        var noteTitle = "Test Note";
        var noteContent = "This is a test note.";
        var noteCreated = DateTime.UtcNow;
        var noteLastModified = DateTime.UtcNow;
        var userId = "user123";

        mockRepo.Setup(r => r.GetAllNotesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Note>
                {
                    new Note
                    {
                        Id = noteId,
                        Title = noteTitle,
                        ContentMarkdown = noteContent,
                        Created = noteCreated,
                        LastModified = noteLastModified,
                        UserId = userId
                    }
                });

        var mockLogger = new Mock<ILogger<GetAllNotesQueryHandler>>();
        var handler = new GetAllNotesQueryHandler(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllNotesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(noteId, result.First().Id);
    }
}