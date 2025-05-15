using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.Application.Categories.Commands;
using NotesApp.Application.Exceptions;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Categories.CommandTests;

public class UpdateCategoryHandlerTests
{
    // Setup: Create 2 test categories with different IDs and user IDs that we can test against.
    // Note: In a real-world scenario, you would likely use a test database or in-memory database for this.
    // For simplicity, we are using in-memory objects here.

    private List<Category> categories =
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
            UserId = "user456",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }
    ];

    [Fact]
    public async Task Handle_ShouldUpdateCategory()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var testCategories = new List<Category>(categories);
        var categoryToUpdate = testCategories[0];
        var userId = categoryToUpdate.UserId;  // "user123"

        var updateCategoryCommand = new UpdateCategoryInternalCommand
        {
            Id = categoryToUpdate.Id,
            Name = "Updated Category"
        };

        // Setup GetCategoryByIdAsync
        mockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testCategories.FirstOrDefault(c => c.Id == id));

        // Setup UpdateCategoryAsync to return the updated category
        mockRepo.Setup(r => r.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category category, CancellationToken _) =>
            {
                var existingCategory = testCategories.FirstOrDefault(c => c.Id == category.Id);
                if (existingCategory != null)
                {
                    existingCategory.Name = category.Name;
                    existingCategory.LastModified = DateTime.UtcNow;
                    return existingCategory;
                }
                return null;
            });

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateCategoryHandler>>();

        var handler = new UpdateCategoryHandler(
            mockRepo.Object,
            mockHttpContextAccessor.Object,
            mockLogger.Object
        );

        // Act
        var result = await handler.Handle(updateCategoryCommand, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Category", result.Name);
        mockRepo.Verify(r => r.UpdateCategoryAsync(
            It.Is<Category>(c => c.Id == categoryToUpdate.Id),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var testCategories = new List<Category>(categories);
        var categoryToUpdate = testCategories[0];

        // Use a different user ID to simulate unauthorized access
        var userId = "unauthorizedUser";

        var updateCategoryCommand = new UpdateCategoryInternalCommand
        {
            Id = categoryToUpdate.Id,
            Name = "Updated Category"
        };

        mockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testCategories.FirstOrDefault(c => c.Id == id));

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateCategoryHandler>>();

        var handler = new UpdateCategoryHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(updateCategoryCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var updateCategoryCommand = new UpdateCategoryInternalCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            Name = "Updated Category"
        };

        mockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => null);

        var userId = "user123"; // Use a valid user ID
        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateCategoryHandler>>();

        var handler = new UpdateCategoryHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.Handle(updateCategoryCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowCategoryOperationException_WhenUpdateFails()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var testCategories = new List<Category>(categories);
        var categoryToUpdate = testCategories[0];

        var userId = categoryToUpdate.UserId;  // This will be "user123"

        var updateCategoryCommand = new UpdateCategoryInternalCommand
        {
            Id = categoryToUpdate.Id,
            Name = "Updated Category"
        };

        mockRepo.Setup(r => r.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                testCategories.FirstOrDefault(c => c.Id == id));

        mockRepo.Setup(r => r.UpdateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CategoryOperationException("Failed to update category with ID " + categoryToUpdate.Id));

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var mockLogger = new Mock<ILogger<UpdateCategoryHandler>>();

        var handler = new UpdateCategoryHandler(mockRepo.Object, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<CategoryOperationException>(async () =>
            await handler.Handle(updateCategoryCommand, CancellationToken.None));
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
