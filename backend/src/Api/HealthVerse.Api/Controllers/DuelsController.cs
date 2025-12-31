using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Application.Commands;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DuelsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DuelsController> _logger;

    public DuelsController(
        IMediator mediator,
        ICurrentUser currentUser,
        ILogger<DuelsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Düello daveti gönder.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateDuelResponse>> CreateDuel([FromBody] CreateDuelRequest request)
    {
        var command = new CreateDuelCommand(
            _currentUser.UserId,
            request.OpponentId,
            request.ActivityType,
            request.TargetMetric,
            request.TargetValue,
            request.DurationDays
        );

        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            if (response.Message.Contains("Rakip bulunamadı")) return NotFound(response);
            return BadRequest(response); // Generic fail message
        }

        return Ok(response);
    }

    /// <summary>
    /// Bekleyen düello davetlerini döner.
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<PendingDuelsResponse>> GetPendingDuels()
    {
        var response = await _mediator.Send(new GetPendingDuelsQuery(_currentUser.UserId));
        return Ok(response);
    }

    /// <summary>
    /// Düello davetini kabul et.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<DuelActionResponse>> AcceptDuel(Guid id)
    {
        var response = await _mediator.Send(new AcceptDuelCommand(id, _currentUser.UserId));

        if (!response.Success)
        {
            if (response.Message == "Düello bulunamadı.") return NotFound(response);
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Düello davetini reddet.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<DuelActionResponse>> RejectDuel(Guid id)
    {
        var response = await _mediator.Send(new RejectDuelCommand(id, _currentUser.UserId));

        if (!response.Success)
        {
             if (response.Message == "Düello bulunamadı.") return NotFound(response);
             return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Aktif düellolarımı döner.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ActiveDuelsResponse>> GetActiveDuels()
    {
        var response = await _mediator.Send(new GetActiveDuelsQuery(_currentUser.UserId));
        return Ok(response);
    }

    /// <summary>
    /// Düello detayını döner.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DuelDetailDto>> GetDuelDetail(Guid id)
    {
        var currentUserId = _currentUser.UserId;
        var response = await _mediator.Send(new GetDuelDetailQuery(id, currentUserId));

        if (response == null) return NotFound();
        if (response.ChallengerId != currentUserId && response.OpponentId != currentUserId) return Forbid();

        return Ok(response);
    }

    /// <summary>
    /// Rakibi dürt (günde bir kez).
    /// </summary>
    [HttpPost("{id:guid}/poke")]
    public async Task<ActionResult<DuelActionResponse>> PokeDuel(Guid id)
    {
        var response = await _mediator.Send(new PokeDuelCommand(id, _currentUser.UserId));

        if (!response.Success)
        {
             if (response.Message == "Düello bulunamadı.") return NotFound(response);
             if (response.Message == "Bu düelloda yetkiniz yok.") return Forbid();
             return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Düello geçmişini döner.
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<DuelHistoryResponse>> GetDuelHistory(int limit = 20)
    {
        var response = await _mediator.Send(new GetDuelHistoryQuery(_currentUser.UserId, limit));
        return Ok(response);
    }
}
