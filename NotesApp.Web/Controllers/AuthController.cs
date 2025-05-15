using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.UseCases.Auth.Commands;

namespace NotesApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Success)
            return Unauthorized(new { result.Message });

        return Ok(new { Message = "Logged in successfully" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Success)
            return BadRequest(new { Errors = result.Errors });

        return Ok(new { Message = "Registered and logged in successfully" });
    }
}
