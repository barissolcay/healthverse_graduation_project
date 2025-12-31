using HealthVerse.Gamification.Application.DTOs;
using HealthVerse.Gamification.Application.Queries;
using HealthVerse.Tasks.Application.Commands;
using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IMediator mediator,
        ICurrentUser currentUser,
        ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının streak detaylarını döner.
    /// </summary>
    [HttpGet("{id:guid}/streak")]
    public async Task<ActionResult<StreakDetailResponse>> GetStreakDetail(Guid id)
    {
        var result = await _mediator.Send(new GetStreakDetailQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının tüm istatistiklerini döner.
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    public async Task<ActionResult<UserStatsResponse>> GetStats(Guid id)
    {
        var result = await _mediator.Send(new GetUserStatsQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının son N günlük puan geçmişini döner.
    /// </summary>
    [HttpGet("{id:guid}/points-history")]
    public async Task<ActionResult<PointsHistoryResponse>> GetPointsHistory(Guid id, int days = 30)
    {
        var result = await _mediator.Send(new GetPointsHistoryQuery(id, days));
        return Ok(result);
    }

    // ===== Interests Endpoints =====

    /// <summary>
    /// Kullanıcının ilgi alanlarını döner.
    /// </summary>
    [HttpGet("interests")]
    public async Task<ActionResult<HealthVerse.Tasks.Application.Queries.InterestsResponse>> GetInterests()
    {
        var result = await _mediator.Send(new GetUserInterestsQuery(_currentUser.UserId));
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının ilgi alanlarını kaydeder (replace all).
    /// </summary>
    [HttpPost("interests")]
    public async Task<ActionResult<UpdateInterestsResponse>> SaveInterests([FromBody] SaveInterestsRequest request)
    {
        var result = await _mediator.Send(new UpdateInterestsCommand(_currentUser.UserId, request.ActivityTypes));
        return Ok(result);
    }
}
