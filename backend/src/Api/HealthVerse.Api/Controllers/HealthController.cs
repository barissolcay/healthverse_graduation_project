using HealthVerse.Contracts.Health;
using HealthVerse.Gamification.Application.Commands;
using HealthVerse.Gamification.Application.DTOs;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVerse.Api.Controllers;

/// <summary>
/// Sağlık verisi senkronizasyonu (adım, kalori vb.).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IMediator mediator,
        ICurrentUser currentUser,
        ILogger<HealthController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Synchronizes all health data from Flutter Health package.
    /// Updates: Steps/Points, Goals, Tasks, Duels, Partner Missions, Global Missions.
    /// Rejects MANUAL and UNKNOWN recording methods automatically.
    /// </summary>
    /// <param name="request">Health activities from Flutter Health</param>
    /// <returns>Sync result with all module updates</returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(HealthSyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthSyncResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HealthSyncResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthSyncResponse>> SyncHealthData([FromBody] SyncHealthDataRequest request)
    {
        // Validation
        if (request.Activities == null || request.Activities.Count == 0)
        {
            return BadRequest(new HealthSyncResponse
            {
                Success = false,
                Message = "En az bir aktivite verisi gerekli."
            });
        }

        if (request.Activities.Count > 100)
        {
            return BadRequest(new HealthSyncResponse
            {
                Success = false,
                Message = "Tek seferde en fazla 100 aktivite gönderilebilir."
            });
        }

        _logger.LogInformation(
            "Health sync started for user {UserId}: {Count} activities",
            _currentUser.UserId, request.Activities.Count);

        var result = await _mediator.Send(new SyncHealthDataCommand(
            _currentUser.UserId,
            request.Activities));

        if (!result.Success && result.Message == "Kullanıcı bulunamadı.")
        {
            return NotFound(result);
        }

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// [Legacy] Günlük adım sayısını senkronize eder.
    /// Yeni entegrasyonlar için sync endpoint'ini kullanın.
    /// </summary>
    [HttpPost("sync-steps")]
    [Obsolete("Use /api/health/sync endpoint instead")]
    public async Task<ActionResult<StepSyncResponse>> SyncSteps([FromBody] SyncStepsRequest request)
    {
        // Validation
        if (request.StepCount < 0)
        {
            return BadRequest(new StepSyncResponse 
            { 
                Success = false, 
                Message = "Adım sayısı negatif olamaz." 
            });
        }

        // UserId artık authenticated user'dan alınıyor - güvenli!
        var result = await _mediator.Send(new SyncStepsCommand(_currentUser.UserId, request.StepCount));
        
        if (!result.Success && result.Message == "Kullanıcı bulunamadı.")
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

/// <summary>
/// Request for syncing health data from Flutter Health package.
/// </summary>
public sealed class SyncHealthDataRequest
{
    /// <summary>
    /// List of health activities collected from device.
    /// Each activity has: ActivityType, TargetMetric, Value, RecordingMethod.
    /// </summary>
    /// <example>
    /// [
    ///   { "activityType": "WALKING", "targetMetric": "STEPS", "value": 8500, "recordingMethod": "AUTOMATIC" },
    ///   { "activityType": "RUNNING", "targetMetric": "DISTANCE", "value": 5200, "recordingMethod": "ACTIVE" },
    ///   { "activityType": "RUNNING", "targetMetric": "CALORIES", "value": 320, "recordingMethod": "AUTOMATIC" }
    /// ]
    /// </example>
    public List<HealthActivityData> Activities { get; init; } = new();
}

/// <summary>
/// Adım senkronizasyon request'i.
/// Not: UserId artık request'te yok, authenticated user'dan alınıyor.
/// [Obsolete] Yeni entegrasyonlar için SyncHealthDataRequest kullanın.
/// </summary>
public sealed class SyncStepsRequest
{
    /// <summary>
    /// Günlük toplam adım sayısı.
    /// </summary>
    public int StepCount { get; init; }
}
