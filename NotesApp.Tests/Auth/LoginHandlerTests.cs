using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NotesApp.Application.Auth.Commands;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Auth;

public class LoginHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly LoginHandler _handler;
    private readonly Mock<IAuthenticationService> _authServiceMock;

    public LoginHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );

        // Setup authentication service
        _authServiceMock = new Mock<IAuthenticationService>();
        _authServiceMock
            .Setup(x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        // Setup service provider
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(_authServiceMock.Object);

        // Setup HttpContext
        var mockHttpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider.Object
        };

        // Setup HttpContextAccessor
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(mockHttpContext);

        // Create the handler instance
        _handler = new LoginHandler(_userManagerMock.Object, _httpContextAccessorMock.Object);
    }

    [Theory]
    [InlineData("john.doe@domain.com", true)]  // Valid email
    [InlineData("john.doedomain.com", false)]  // Invalid email - missing @
    [InlineData("", false)]                    // Invalid email - empty
    [InlineData("test@", false)]              // Invalid email - incomplete
    public async Task Handle_ValidatesEmailFormat(string email, bool shouldSucceed)
    {
        // Arrange
        var command = new LoginCommand(email, "Password123!");
        var user = shouldSucceed ? new User
        {
            Email = email,
            UserName = email,
            Id = Guid.NewGuid().ToString()
        } : null;

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        if (shouldSucceed)
        {
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user!, command.Password))
                .ReturnsAsync(true);
        }

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(shouldSucceed, result.Success);

        // Verify sign in was only called for successful cases
        _authServiceMock.Verify(x => x.SignInAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties>()),
            Times.Exactly(shouldSucceed ? 1 : 0));
    }

    [Theory]
    [InlineData("correct123!", true)]  // Valid password that matches
    [InlineData("wrong123!", false)]   // Valid format but wrong password
    [InlineData("", false)]            // Empty password
    public async Task Handle_ValidatesPassword(string password, bool shouldSucceed)
    {
        // Arrange
        const string validEmail = "test@example.com";
        var user = new User
        {
            Email = validEmail,
            UserName = validEmail,
            Id = Guid.NewGuid().ToString()
        };

        var command = new LoginCommand(validEmail, password);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(validEmail))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(password == "correct123!"); // Only succeed with correct password

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(shouldSucceed, result.Success);
        if (!shouldSucceed)
        {
            Assert.Contains("Invalid credentials", result.Message);
        }

        // Verify sign in was only called for successful cases
        _authServiceMock.Verify(x => x.SignInAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties>()),
            Times.Exactly(shouldSucceed ? 1 : 0));
    }
}
