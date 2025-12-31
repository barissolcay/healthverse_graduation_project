using HealthVerse.Competition.Application.Commands;
using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Competition.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeagueController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<LeagueController> _logger;

    public LeagueController(
        IMediator mediator,
        ICurrentUser currentUser,
        ILogger<LeagueController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının mevcut lig odasını döner.
    /// </summary>
    [HttpGet("my-room")]
    public async Task<ActionResult<MyRoomResponse>> GetMyRoom()
    {
        var result = await _mediator.Send(new GetMyRoomQuery(_currentUser.UserId));

        if (result is null)
            return NotFound(new { Message = "Bu hafta bir lige katılmadınız." });

        return Ok(result);
    }

    /// <summary>
    /// Belirli bir odanın sıralamasını döner.
    /// </summary>
    [HttpGet("room/{roomId:guid}/leaderboard")]
    public async Task<ActionResult<RoomLeaderboardResponse>> GetRoomLeaderboard(Guid roomId)
    {
        var result = await _mediator.Send(new GetRoomLeaderboardQuery(roomId, _currentUser.UserId));

        if (result is null)
            return NotFound(new { Message = "Oda bulunamadı." });

        return Ok(result);
    }

    /// <summary>
    /// Tüm tier listesi ve kurallarını döner.
    /// </summary>
    [HttpGet("tiers")]
    public async Task<ActionResult<TiersResponse>> GetTiers()
    {
        var result = await _mediator.Send(new GetTiersQuery(_currentUser.UserId));
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının geçmiş hafta sonuçlarını döner.
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<LeagueHistoryResponse>> GetHistory(int limit = 10)
    {
        var result = await _mediator.Send(new GetLeagueHistoryQuery(_currentUser.UserId, limit));
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcıyı bu haftanın ligine dahil eder.
    /// </summary>
    [HttpPost("join")]
    public async Task<ActionResult<JoinLeagueResponse>> JoinLeague()
    {
        var result = await _mediator.Send(new JoinLeagueCommand(_currentUser.UserId));

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
