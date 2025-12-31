using HealthVerse.Gamification.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace HealthVerse.Api.Controllers;

/// <summary>
/// Puan sıralama tabloları.
/// Public endpoint - herkes görebilir, authenticated kullanıcılar kendi sıralamalarını da görür.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Intentionally public - leaderboards are visible to everyone
public class LeaderboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(
        IMediator mediator,
        ICurrentUser currentUser,
        ILogger<LeaderboardController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Haftalık puan sıralaması (mevcut hafta).
    /// </summary>
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyLeaderboard(int limit = 50)
    {
        var result = await _mediator.Send(new GetLeaderboardQuery("WEEKLY", GetCurrentUserIdOrNull(), limit));
        return Ok(result);
    }

    /// <summary>
    /// Aylık puan sıralaması (mevcut ay).
    /// </summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyLeaderboard(int limit = 50)
    {
        var result = await _mediator.Send(new GetLeaderboardQuery("MONTHLY", GetCurrentUserIdOrNull(), limit));
        return Ok(result);
    }

    /// <summary>
    /// Tüm zamanlar sıralaması (TotalPoints bazlı).
    /// </summary>
    [HttpGet("alltime")]
    public async Task<IActionResult> GetAllTimeLeaderboard(int limit = 100)
    {
        var result = await _mediator.Send(new GetLeaderboardQuery("ALLTIME", GetCurrentUserIdOrNull(), limit));
        return Ok(result);
    }

    private Guid? GetCurrentUserIdOrNull()
    {
        return _currentUser.IsAuthenticated ? _currentUser.UserId : null;
    }
}
