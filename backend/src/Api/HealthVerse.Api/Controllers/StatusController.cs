using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthVerse.Api.Controllers;

/// <summary>
/// Sistem durumu ve sağlık kontrolleri.
/// </summary>
[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StatusController> _logger;

    public StatusController(
        IMediator mediator,
        ILogger<StatusController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Temel sağlık kontrolü.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Detaylı sağlık kontrolü (veritabanı dahil).
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        var result = await _mediator.Send(new HealthVerse.Api.Application.Queries.GetSystemStatusQuery());
        return Ok(result);
    }

    /// <summary>
    /// Liveness probe (k8s için).
    /// </summary>
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "Alive" });
    }

    /// <summary>
    /// Readiness probe (k8s için).
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        // Reuse the main query but just check status
        var result = await _mediator.Send(new HealthVerse.Api.Application.Queries.GetSystemStatusQuery());
        
        if (result.Status == "Healthy")
        {
            return Ok(new { status = "Ready" });
        }
        
        return StatusCode(503, new { status = "NotReady", reason = "System check failed" });
    }
}
