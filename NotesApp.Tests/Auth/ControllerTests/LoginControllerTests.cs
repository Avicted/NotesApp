using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Application.UseCases.Auth.Commands;
using NotesApp.Web.Controllers;

namespace NotesApp.Tests.Auth.ControllerTests;

public class LoginControllerTests
{
    [Fact]
    public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new AuthController(mediatorMock.Object);
        var command = new LoginCommand("john.doe@domain.com", "Password123!");

        mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResult(true, null));

        // Act
        var result = await controller.Login(command);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Logged in successfully", response.Value?.GetType().GetProperty("Message")?.GetValue(response.Value));
    }

    [Theory]
    [InlineData("john.doe@domain.com", "Password123!")]
    [InlineData("john.doe.domain.com", "Password123!")] // Invalid email
    [InlineData("", "Password123!")]                    // Invalid email
    [InlineData("john.doe@domain.com", "password")]     // Invalid password
    [InlineData("john.doe@domain.com", "")]             // Invalid password
    public async Task Login_ShouldReturnBadRequest_WhenLoginFails(string? email, string? password)
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new AuthController(mediatorMock.Object);

        var command = new LoginCommand(email ?? string.Empty, password ?? string.Empty);

        mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResult(false, "Invalid credentials"));

        // Act
        var result = await controller.Login(command);
        var badRequestResult = Assert.IsType<UnauthorizedObjectResult>(result);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, badRequestResult.StatusCode);
        var response = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials", response.Value?.GetType().GetProperty("Message")?.GetValue(response.Value));
    }
}
