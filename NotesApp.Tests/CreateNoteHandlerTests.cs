using Microsoft.AspNetCore.Http;
using Moq;
using NotesApp.Application.Notes.Commands;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests;

public class CreateNoteHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateNote()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var noteId = Guid.NewGuid();

        var createNoteCommand = new CreateNoteCommand
        {
            Title = "Test Note",
            ContentMarkdown = "This is a test note.",
        };

        mockRepo.Setup(r => r.CreateNoteAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Note note, CancellationToken _) => note);

        var userId = Guid.NewGuid().ToString();
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);

        var handler = new CreateNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object);

        // Act
        await handler.Handle(createNoteCommand, CancellationToken.None);

        // Assert
        mockRepo.Verify(r => r.CreateNoteAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var mockRepo = new Mock<INoteRepository>();
        var createNoteCommand = new CreateNoteCommand
        {
            Title = "Test Note",
            ContentMarkdown = "This is a test note.",
        };

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser("");

        var handler = new CreateNoteHandler(mockRepo.Object, mockHttpContextAccessor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(createNoteCommand, CancellationToken.None));
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
