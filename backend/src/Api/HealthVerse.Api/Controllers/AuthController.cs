using HealthVerse.Identity.Application.Commands;
using HealthVerse.Identity.Application.DTOs;
using HealthVerse.Identity.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUser _currentUser;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger,
        IWebHostEnvironment env,
        ICurrentUser currentUser)
    {
        _mediator = mediator;
        _logger = logger;
        _env = env;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Yeni kullanıcı kaydı (Firebase token ile).
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.IdToken, request.Username, request.AvatarId);
        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Giriş (Firebase token ile).
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.IdToken);
        var response = await _mediator.Send(command);

        if (!response.Success && response.Message.Contains("Geçersiz"))
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Mevcut kullanıcı bilgisi.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
    {
        var query = new GetCurrentUserQuery(_currentUser.UserId);
        var response = await _mediator.Send(query);

        if (response == null)
        {
            return NotFound(new { error = "Kullanıcı bulunamadı." });
        }

        return Ok(response);
    }

    // ===== Development-Only Test Endpoints =====

    /// <summary>
    /// [DEV ONLY] Test kayıt - Firebase bypass.
    /// </summary>
    [HttpPost("dev-register")]
    public async Task<ActionResult<RegisterResponse>> DevRegister([FromBody] DevRegisterRequest request)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        var command = new DevRegisterCommand(request.Username, request.Email, request.AvatarId);
        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// [DEV ONLY] Test login - Firebase bypass. Returns userId for X-User-Id header.
    /// </summary>
    [HttpPost("dev-login")]
    public async Task<ActionResult<DevLoginResponse>> DevLogin([FromBody] DevLoginRequest request)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        var command = new DevLoginCommand(request.Email);
        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
