using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.Application.Categories.Commands;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Categories.CommandTests;

public class DeleteCategoryHandlerTests
{
    private List<Category> _categories =
    [
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category 1",
            UserId = "user123",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
            },
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category 2",
            UserId =    "user456",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }
    ];

    [Fact]
    public async Task Handle_ShouldDeleteCategory()
    {
        // Arrange
        var categoryMockRepo = new Mock<ICategoryRepository>();
        var noteMockRepo = new Mock<INoteRepository>();

        var testCategories = new List<Category>(_categories);
        var categoryToDelete = testCategories[0];
        var userId = categoryToDelete.UserId;  // "user123"

        var deleteCategoryCommand = new DeleteCategoryCommand
        {
            Id = categoryToDelete.Id,
        };

        // Setup GetCategoryByIdAsync
        categoryMockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testCategories.FirstOrDefault(c => c.Id == id));

        // Setup DeleteCategoryAsync to remove the category
        categoryMockRepo.Setup(r => r.DeleteCategoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var category = testCategories.FirstOrDefault(c => c.Id == id);
                if (category != null)
                {
                    testCategories.Remove(category);
                    return true;
                }
                return false;
            });

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var loggerMock = new Mock<ILogger<DeleteCategoryHandler>>();
        var handler = new DeleteCategoryHandler(categoryMockRepo.Object, noteMockRepo.Object, mockHttpContextAccessor.Object, loggerMock.Object);

        // Act
        await handler.Handle(deleteCategoryCommand, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(testCategories, c => c.Id == categoryToDelete.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryMockRepo = new Mock<ICategoryRepository>();
        var noteMockRepo = new Mock<INoteRepository>();

        var testCategories = new List<Category>(_categories);
        var nonExistentCategoryId = Guid.NewGuid();
        var userId = "user123";

        var deleteCategoryCommand = new DeleteCategoryCommand
        {
            Id = nonExistentCategoryId,
        };

        // Setup GetCategoryByIdAsync to return null
        categoryMockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testCategories.FirstOrDefault(c => c.Id == id));

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var loggerMock = new Mock<ILogger<DeleteCategoryHandler>>();
        var handler = new DeleteCategoryHandler(categoryMockRepo.Object, noteMockRepo.Object, mockHttpContextAccessor.Object, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.Handle(deleteCategoryCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotOwnCategory()
    {
        // Arrange
        var categoryMockRepo = new Mock<ICategoryRepository>();
        var noteMockRepo = new Mock<INoteRepository>();

        var testCategories = new List<Category>(_categories);
        var categoryToDelete = testCategories[0];
        var userId = "unauthorizedUser";  // Different user ID

        var deleteCategoryCommand = new DeleteCategoryCommand
        {
            Id = categoryToDelete.Id,
        };

        // Setup GetCategoryByIdAsync to return the category
        categoryMockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testCategories.FirstOrDefault(c => c.Id == id));

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var loggerMock = new Mock<ILogger<DeleteCategoryHandler>>();
        var handler = new DeleteCategoryHandler(categoryMockRepo.Object, noteMockRepo.Object, mockHttpContextAccessor.Object, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(deleteCategoryCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenUserNotAuthenticated()
    {
        // Arrange
        var categoryMockRepo = new Mock<ICategoryRepository>();
        var noteMockRepo = new Mock<INoteRepository>();

        var testCategories = new List<Category>(_categories);
        var categoryToDelete = testCategories[0];

        var deleteCategoryCommand = new DeleteCategoryCommand
        {
            Id = categoryToDelete.Id,
        };

        // Setup GetCategoryByIdAsync to return the category
        categoryMockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testCategories.FirstOrDefault(c => c.Id == id));

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(""); // No user
        var loggerMock = new Mock<ILogger<DeleteCategoryHandler>>();
        var handler = new DeleteCategoryHandler(categoryMockRepo.Object, noteMockRepo.Object, mockHttpContextAccessor.Object, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(deleteCategoryCommand, CancellationToken.None));
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
