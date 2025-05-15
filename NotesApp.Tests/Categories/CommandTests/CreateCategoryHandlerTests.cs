using Microsoft.AspNetCore.Http;
using Moq;
using NotesApp.Application.UseCases.Categories.Commands;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Categories.CommandTests;

public class CreateCategoryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCategory()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var categoryId = Guid.NewGuid();



        var createCategoryCommand = new CreateCategoryInternalCommand
        {
            Name = "Test Category",
        };

        mockRepo.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category category, CancellationToken _) => category);

        var userId = Guid.NewGuid().ToString();
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);

        var handler = new CreateCategoryHandler(mockRepo.Object, mockHttpContextAccessor.Object);

        // Act
        await handler.Handle(createCategoryCommand, CancellationToken.None);

        // Assert
        mockRepo.Verify(r => r.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var createCategoryCommand = new CreateCategoryInternalCommand
        {
            Name = "Test Category",
        };

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser("");

        var handler = new CreateCategoryHandler(mockRepo.Object, mockHttpContextAccessor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(createCategoryCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowIfCategoryAlreadyExists()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var createCategoryCommand = new CreateCategoryInternalCommand
        {
            Name = "Test Category",
        };

        var userId = Guid.NewGuid().ToString();
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);

        mockRepo.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Category already exists."));

        var handler = new CreateCategoryHandler(mockRepo.Object, mockHttpContextAccessor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(createCategoryCommand, CancellationToken.None));
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
