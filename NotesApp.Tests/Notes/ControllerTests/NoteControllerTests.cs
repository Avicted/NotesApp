using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Web.Controllers;
using NotesApp.Application.UseCases.Notes.Queries;
using NotesApp.Application.DTOs;
using NotesApp.Application.UseCases.Notes.Commands;
using System.Security.Claims;

namespace NotesApp.Tests.Notes.ControllerTests;

public class NoteControllerTests
{
    // Create
    // Create wihout category
    [Fact]
    public async Task CreateNote_ShouldReturnOk_WhenNoteIsCreated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteDto = new NoteDto { Title = "Test Note", Content = "Test Content" };
        var userId = Guid.NewGuid();

        // Set up the user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        mediatorMock.Setup(m => m.Send(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NoteDto { Id = Guid.NewGuid(), Title = noteDto.Title, Content = noteDto.Content });

        // Act
        var createNoteCommand = new CreateNoteCommand
        {
            Title = noteDto.Title,
            ContentMarkdown = noteDto.Content,
        };
        var result = await controller.CreateNote(createNoteCommand);

        // Assert
        var okResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdNote = Assert.IsType<NoteDto>(okResult.Value);
        Assert.Equal(noteDto.Title, createdNote.Title);
    }

    // Create with category
    [Fact]
    public async Task CreateNote_ShouldReturnOk_WhenNoteWithCategoryIsCreated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var categoryId = Guid.NewGuid();
        var noteDto = new NoteDto { Title = "Test Note", Content = "Test Content", CategoryId = categoryId.ToString() };
        var userId = Guid.NewGuid();

        // Set up the user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        mediatorMock.Setup(m => m.Send(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NoteDto { Id = Guid.NewGuid(), Title = noteDto.Title, Content = noteDto.Content, CategoryId = noteDto.CategoryId });

        // Act
        var createNoteCommand = new CreateNoteCommand
        {
            Title = noteDto.Title,
            ContentMarkdown = noteDto.Content,
            CategoryId = categoryId
        };

        var result = await controller.CreateNote(createNoteCommand);

        // Assert
        var okResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdNote = Assert.IsType<NoteDto>(okResult.Value);
        Assert.Equal(noteDto.Title, createdNote.Title);
        Assert.Equal(noteDto.CategoryId, createdNote.CategoryId);
    }

    // Create with empty title
    [Fact]
    public async Task CreateNote_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteDto = new NoteDto { Title = "", Content = "Test Content" };
        var userId = Guid.NewGuid();

        // Set up the user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        // Act
        var createNoteCommand = new CreateNoteCommand
        {
            Title = noteDto.Title,
            ContentMarkdown = noteDto.Content,
        };
        var result = await controller.CreateNote(createNoteCommand);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Read
    [Fact]
    public async Task GetNoteById_ShouldReturnOk_WhenNoteExists()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteId = Guid.NewGuid();
        var noteDto = new NoteDto { Id = noteId, Title = "Test Note", Content = "Test Content" };

        mediatorMock.Setup(m => m.Send(It.IsAny<GetNoteByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(noteDto);

        // Act
        var result = await controller.GetNoteById(noteId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedNote = Assert.IsType<NoteDto>(okResult.Value);
        Assert.Equal(noteDto.Title, returnedNote.Title);
    }

    [Fact]
    public async Task GetNoteById_ShouldReturnNotFound_WhenNoteDoesNotExist()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteId = Guid.NewGuid();
        var noteDto = new NoteDto { Id = noteId, Title = "Test Note", Content = "Test Content" };

        mediatorMock.Setup(m => m.Send(It.IsAny<GetNoteByIdQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<NoteDto?>(null));

        // Act
        var result = await controller.GetNoteById(noteId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // Update
    [Fact]
    public async Task UpdateNote_ShouldReturnOk_WhenNoteIsUpdated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteId = Guid.NewGuid();
        var noteDto = new NoteDto { Id = noteId, Title = "Updated Note", Content = "Updated Content" };

        mediatorMock.Setup(m => m.Send(
        It.Is<UpdateNoteInternalCommand>(c =>
            c.Id == noteId &&
            c.Title == noteDto.Title &&
            c.Content == noteDto.Content),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(noteDto);

        // Act
        var updateNoteCommand = new UpdateNoteCommand
        {
            Title = noteDto.Title,
            Content = noteDto.Content,
            CategoryId = noteDto.CategoryId
        };

        var result = await controller.UpdateNote(noteId, updateNoteCommand);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedNote = Assert.IsType<NoteDto>(okResult.Value);
        Assert.Equal(noteDto.Title, updatedNote.Title);
    }

    [Fact]
    public async Task UpdateNote_ShouldReturnNotFound_WhenNoteDoesNotExist()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteId = Guid.NewGuid();
        var noteDto = new NoteDto { Id = noteId, Title = "Updated Note", Content = "Updated Content" };

        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateNoteInternalCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<NoteDto?>(null));

        // Act
        var updateNoteCommand = new UpdateNoteCommand
        {
            Title = noteDto.Title,
            Content = noteDto.Content,
            CategoryId = noteDto.CategoryId
        };

        var result = await controller.UpdateNote(noteId, updateNoteCommand);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // Delete
    [Fact]
    public async Task DeleteNote_ShouldReturnOk_WhenNoteIsDeleted()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteId = Guid.NewGuid();

        mediatorMock.Setup(m => m.Send(It.IsAny<DeleteNoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        // Act
        var result = await controller.DeleteNote(noteId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteNote_ShouldReturnNotFound_WhenNoteDoesNotExist()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var controller = new NotesController(mediatorMock.Object, mockHttpContextAccessor.Object);
        var noteId = Guid.NewGuid();

        mediatorMock.Setup(m => m.Send(It.IsAny<DeleteNoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(false));

        // Act
        var result = await controller.DeleteNote(noteId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
