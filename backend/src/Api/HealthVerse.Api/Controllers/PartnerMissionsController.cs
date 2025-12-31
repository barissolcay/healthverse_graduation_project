using HealthVerse.Missions.Application.Commands;
using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/missions/partner")]
[Authorize]
public class PartnerMissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<PartnerMissionsController> _logger;

    public PartnerMissionsController(IMediator mediator, ICurrentUser currentUser, ILogger<PartnerMissionsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Bu hafta boşta olan arkadaşları döner.
    /// </summary>
    [HttpGet("available-friends")]
    public async Task<ActionResult<AvailableFriendsResponse>> GetAvailableFriends()
    {
        var response = await _mediator.Send(new GetAvailableFriendsQuery(_currentUser.UserId));
        return Ok(response);
    }

    /// <summary>
    /// Arkadaşla partner görevi başlat.
    /// </summary>
    [HttpPost("pair/{friendId:guid}")]
    public async Task<ActionResult<PairPartnerResponse>> PairWithFriend(Guid friendId)
    {
        var response = await _mediator.Send(new PairWithFriendCommand(_currentUser.UserId, friendId));

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Aktif partner görevimi döner.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<PartnerMissionDetailDto?>> GetActivePartnerMission()
    {
        var response = await _mediator.Send(new GetActivePartnerMissionQuery(_currentUser.UserId));
        return Ok(response);
    }

    /// <summary>
    /// Partneri dürt (günde 1 kez).
    /// </summary>
    [HttpPost("{id:guid}/poke")]
    public async Task<ActionResult<PartnerMissionActionResponse>> PokeMission(Guid id)
    {
        var response = await _mediator.Send(new PokePartnerCommand(id, _currentUser.UserId));

        if (!response.Success)
        {
            if (response.Message == "Görev bulunamadı.")
                return NotFound(response);
                
            if (response.Message == "Bu göreve dahil değilsiniz.")
                 return Forbid();

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Partner görevi geçmişi.
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<PartnerMissionHistoryResponse>> GetHistory(int limit = 10)
    {
        var response = await _mediator.Send(new GetPartnerMissionHistoryQuery(_currentUser.UserId, limit));
        return Ok(response);
    }
}
