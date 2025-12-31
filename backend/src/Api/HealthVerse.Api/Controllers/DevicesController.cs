using HealthVerse.Contracts.Notifications;
using HealthVerse.Notifications.Application.Commands;
using HealthVerse.Notifications.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        IMediator mediator,
        ICurrentUser currentUser,
        ILogger<DevicesController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Push token kaydet.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<RegisterDeviceResponse>> RegisterDevice([FromBody] RegisterDeviceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Platform) ||
            (request.Platform.ToUpperInvariant() != DevicePlatform.IOS && 
             request.Platform.ToUpperInvariant() != DevicePlatform.ANDROID))
        {
            return BadRequest(new RegisterDeviceResponse
            {
                Success = false,
                Message = "Platform 'IOS' veya 'ANDROID' olmalıdır."
            });
        }

        var result = await _mediator.Send(new RegisterDeviceCommand(
            request.PushToken,
            request.Platform,
            request.DeviceModel ?? string.Empty,
            request.AppVersion ?? string.Empty,
            _currentUser.UserId
        ));

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Push token sil.
    /// </summary>
    [HttpDelete("{token}")]
    public async Task<ActionResult<DeleteDeviceResponse>> DeleteDevice(string token)
    {
        var result = await _mediator.Send(new UnregisterDeviceCommand(token, _currentUser.UserId));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
