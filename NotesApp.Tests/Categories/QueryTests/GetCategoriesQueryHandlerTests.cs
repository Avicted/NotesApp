using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NotesApp.Application.Categories.Queries;
using NotesApp.Application.DTOs;
using NotesApp.Application.Interfaces;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Categories.QueryTests;

public class GetCategoriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCategoriesForUser()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var userId = Guid.NewGuid().ToString();
        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                UserId = userId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            }
        };

        mockRepo.Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var handler = new GetAllCategoriesQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal("Test Category", result.First().Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCategories()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var userId = Guid.NewGuid().ToString();

        mockRepo.Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Category>());

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var handler = new GetAllCategoriesQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var userId = Guid.NewGuid().ToString();

        mockRepo.Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var handler = new GetAllCategoriesQueryHandler(mockRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoriesWithCorrectProperties()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var userId = Guid.NewGuid().ToString();
        var categoryName = "Test Category";

        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryName,
                UserId = userId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            }
        };

        mockRepo.Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var handler = new GetAllCategoriesQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(categoryName, result.First().Name);
        Assert.IsType<List<CategoryDto>>(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoriesInCorrectOrder()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var userId = Guid.NewGuid().ToString();

        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Category 2",
                UserId = userId,
                Created = DateTime.UtcNow.AddDays(-1),
                LastModified = DateTime.UtcNow.AddDays(-1)
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Category 1",
                UserId = userId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            }
        };

        mockRepo.Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var handler = new GetAllCategoriesQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Category 2", result.First().Name);
        Assert.Equal("Category 1", result.Last().Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoryWithCorrectId()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var userId = Guid.NewGuid().ToString();
        var categoryId = Guid.NewGuid();
        var categoryName = "Test Category";

        var categories = new List<Category>
        {
            new Category
            {
                Id = categoryId,
                Name = categoryName,
                UserId = userId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            }
        };

        mockRepo.Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

        var mockHttpContextAccessor = CreateMockHttpContextAccessorWithUser(userId);
        var handler = new GetAllCategoriesQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(categoryId, result.First().Id);
    }

    private Mock<IHttpContextAccessor> CreateMockHttpContextAccessorWithUser(string userId)
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        });

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

        return httpContextAccessorMock;
    }
}
