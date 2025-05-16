using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Web.Controllers;
using NotesApp.Application.UseCases.Categories.Queries;
using NotesApp.Application.DTOs;
using NotesApp.Application.UseCases.Categories.Commands;
using System.Security.Claims;

namespace NotesApp.Tests.Categories.ControllerTests;

public class CategoryControllerTests
{
    // Create
    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_WhenCategoryIsCreated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var userId = Guid.NewGuid().ToString();

        // Mock ClaimsPrincipal and HttpContext to provide userId
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        // Include UserId in both command and response
        var command = new CreateCategoryCommand
        {
            Name = "Test Category",
        };

        var createdCategory = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
        };

        // Update mock setup to verify the command
        mediatorMock.Setup(m => m.Send(
            It.Is<CreateCategoryInternalCommand>(c =>
                c.Name == command.Name &&
                c.UserId == userId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await controller.CreateCategory(command);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var returnedCategory = Assert.IsType<CategoryDto>(createdResult.Value);
        Assert.Equal(command.Name, returnedCategory.Name);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenCategoryCreationFails()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var command = new CreateCategoryCommand
        {
            Name = "Test Category",
        };

        // Setup mock to return null
        mediatorMock.Setup(m => m.Send(
            It.IsAny<CreateCategoryInternalCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await controller.CreateCategory(command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    // Read
    [Fact]
    public async Task GetCategories_ShouldReturnOk_WhenCategoriesAreFound()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var expectedCategories = new List<CategoryDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Category" },
            new() { Id = Guid.NewGuid(), Name = "Another Category" }
        };

        // Setup mock for query type
        mediatorMock.Setup(m => m.Send(
            It.IsAny<GetAllCategoriesQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await controller.GetAllCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var response = Assert.IsType<List<CategoryDto>>(okResult.Value);
        Assert.NotEmpty(response);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnNotFound_WhenNoCategoriesAreFound()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        // Setup mock for query type
        mediatorMock.Setup(m => m.Send(
            It.IsAny<GetAllCategoriesQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await controller.GetAllCategories();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnOk_WhenCategoryIsFound()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();
        var expectedCategory = new CategoryWithNotesDto { Id = categoryId, Name = "Test Category" };

        // Setup mock for query type
        mediatorMock.Setup(m => m.Send(
            It.IsAny<GetCategoryByIdQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategory);

        // Act
        var result = await controller.GetCategoryById(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<CategoryWithNotesDto>(okResult.Value);
        Assert.Equal(expectedCategory.Id, response.Id);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFound_WhenCategoryIsNotFound()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);
        var categoryId = Guid.NewGuid();

        mediatorMock.Setup(m => m.Send(
           It.IsAny<GetCategoryByIdQuery>(),
           It.IsAny<CancellationToken>()))
       .ReturnsAsync((CategoryWithNotesDto?)null);

        // Act
        var result = await controller.GetCategoryById(categoryId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnCategoryWithNoNotes()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);
        var categoryId = Guid.NewGuid();

        var expectedCategory = new CategoryWithNotesDto { Id = categoryId, Name = "Test Category", Notes = null };

        // Setup mock for query type
        mediatorMock.Setup(m => m.Send(
            It.IsAny<GetCategoryByIdQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategory);

        // Act
        var result = await controller.GetCategoryById(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<CategoryWithNotesDto>(okResult.Value);
        Assert.Equal(expectedCategory.Id, response.Id);
    }

    // Update
    [Fact]
    public async Task UpdateCategory_ShouldReturnOk_WhenCategoryIsUpdated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var existingCategoryId = Guid.NewGuid();
        var existingCategory = new CategoryDto
        {
            Id = existingCategoryId,
            Name = "Existing Category"
        };

        var updatedCategory = new CategoryDto
        {
            Id = existingCategoryId,
            Name = "Updated Category"
        };

        var command = new UpdateCategoryCommand
        {
            Name = "Updated Category"
        };

        // Setup mock for command type
        mediatorMock.Setup(m => m.Send(
            It.IsAny<UpdateCategoryInternalCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCategory);

        // Setup mock for HttpContext to provide userId
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        // Act
        var result = await controller.UpdateCategory(existingCategoryId, command);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var response = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(updatedCategory.Id, response.Id);
        Assert.Equal(updatedCategory.Name, response.Name);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryIsNotFound()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            Name = "Updated Category"
        };

        // Setup mock to return null
        mediatorMock.Setup(m => m.Send(
            It.IsAny<UpdateCategoryInternalCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await controller.UpdateCategory(categoryId, command);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnBadRequest_WhenCategoryUpdateFails()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            Name = "Updated Category"
        };

        // Setup mock to return null
        mediatorMock.Setup(m => m.Send(
            It.IsAny<UpdateCategoryInternalCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await controller.UpdateCategory(categoryId, command);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    // Delete
    [Fact]
    public async Task DeleteCategory_ShouldReturnOk_WhenCategoryIsDeleted()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();

        // Setup mock for command type
        mediatorMock.Setup(m => m.Send(
            It.IsAny<DeleteCategoryCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await controller.DeleteCategory(categoryId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNotFound_WhenCategoryIsNotFound()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();

        // Setup mock to return false
        mediatorMock.Setup(m => m.Send(
            It.IsAny<DeleteCategoryCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await controller.DeleteCategory(categoryId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnBadRequest_WhenCategoryDeletionFails()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();

        // Setup mock to return false
        mediatorMock.Setup(m => m.Send(
            It.IsAny<DeleteCategoryCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await controller.DeleteCategory(categoryId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnInvalidOperationException_WhenCategoryDeletionFails()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();

        // Setup mock to throw exception
        mediatorMock.Setup(m => m.Send(
            It.IsAny<DeleteCategoryCommand>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Category deletion failed."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.DeleteCategory(categoryId));
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnInvalidOperationException_WhenCategoryContainsNotes()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var controller = new CategoriesController(mediatorMock.Object, httpContextAccessorMock.Object);

        var categoryId = Guid.NewGuid();

        // Setup mock to throw exception
        mediatorMock.Setup(m => m.Send(
            It.IsAny<DeleteCategoryCommand>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Category contains notes."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.DeleteCategory(categoryId));
    }
}
