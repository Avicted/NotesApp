using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Application.UseCases.Auth.Commands;
using NotesApp.Web.Controllers;

namespace NotesApp.Tests.Auth.ControllerTests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new AuthController(mediatorMock.Object);
        var command = new RegisterCommand
        {
            Email = "john.doe@domain.com",
            Password = "Password123!",
            Username = "johndoe"
        };

        mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterResult { Success = true });

        // Act
        var result = await controller.Register(command);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Registered and logged in successfully", response.Value?.GetType().GetProperty("Message")?.GetValue(response.Value));
    }

    // Invalid email and or password
    [Theory]
    [InlineData("john.doe@domain.com", "Password123!", "johndoe")]
    [InlineData("john.doe.domain.com", "Password123!", "johndoe")]  // Invalid email
    [InlineData("john.doe@domain.com", "password", "johndoe")]      // Invalid password
    [InlineData("john.doe@domain.com", "Password123!", "")]         // Invalid username
    public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails(string? email, string? password, string? username)
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new AuthController(mediatorMock.Object);

        var command = new RegisterCommand
        {
            Email = email ?? string.Empty,
            Password = password ?? string.Empty,
            Username = username ?? string.Empty
        };

        mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterResult { Success = false, Errors = ["Invalid registration details"] });

        // Act
        var result = await controller.Register(command);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BadRequestObjectResult>(result);
        var errors = response.Value?.GetType().GetProperty("Errors")?.GetValue(response.Value) as IEnumerable<string>;
        var error = errors?.FirstOrDefault();
        var errorMessage = error ?? string.Empty;
        var expectedErrorMessage = "Invalid registration details";

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal(expectedErrorMessage, errorMessage);
        Assert.NotNull(errors);
        Assert.NotEmpty(errors);
        Assert.Contains(expectedErrorMessage, errors!);
        Assert.Equal(expectedErrorMessage, errorMessage);
    }
}
