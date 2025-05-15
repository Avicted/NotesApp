using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NotesApp.Application.Auth.Commands;
using NotesApp.Domain.Entities;

namespace NotesApp.Tests.Auth;

public class RegisterHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly RegisterHandler _handler;
    private readonly Mock<IAuthenticationService> _authServiceMock;


    public RegisterHandlerTests()
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
        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(mockHttpContext);

        _handler = new RegisterHandler(_userManagerMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_UserAlreadyExists_ReturnsError()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "john.doe@domain.com",
            Password = "Password123!",
            Username = "johndoe"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "johndoe",
            Email = "john.doe@domain.com"
        };

        _userManagerMock.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var error = result.Errors?.FirstOrDefault();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task Handle_UserCreatedSuccessfully_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test123!",
            Username = "testuser"
        };

        _userManagerMock.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        _authServiceMock.Verify(x => x.SignInAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Theory]
    [InlineData("notarealemail", "Invalid email address")]
    [InlineData("missing@tld", "Invalid email address")]
    [InlineData("space in@email.com", "Invalid email address")]
    [InlineData("valid.email@domain.com", null)] // Should not fail for valid email
    public async Task Handle_ValidatesEmailFormat(string email, string? expectedError)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = email,
            Password = "Test123!",
            Username = "johndoe"
        };

        _userManagerMock.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Set up the validator
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
        .ReturnsAsync((User _, string _) =>
        {
            // Validate the email from command instead of user object
            if (!IsValidEmail(command.Email))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidEmail",
                    Description = "Invalid email address"
                });
            }
            return IdentityResult.Success;
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (expectedError != null)
        {
            Assert.False(result.Success);
            Assert.Contains(expectedError, result.Errors ?? []);
        }
        else
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
        }
    }

    // https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            static string DomainMapper(System.Text.RegularExpressions.Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
