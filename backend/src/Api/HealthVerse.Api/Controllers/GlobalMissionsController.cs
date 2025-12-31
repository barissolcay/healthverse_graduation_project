using HealthVerse.Missions.Application.Commands;
using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/missions/global")]
[Authorize]
public class GlobalMissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<GlobalMissionsController> _logger;

    public GlobalMissionsController(IMediator mediator, ICurrentUser currentUser, ILogger<GlobalMissionsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Aktif global görevleri döner.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ActiveGlobalMissionsResponse>> GetActiveMissions()
    {
        var response = await _mediator.Send(new GetActiveGlobalMissionsQuery(_currentUser.UserId));
        return Ok(response);
    }

    /// <summary>
    /// Global göreve katıl.
    /// </summary>
    [HttpPost("{id:guid}/join")]
    public async Task<ActionResult<JoinMissionResponse>> JoinMission(Guid id)
    {
        var response = await _mediator.Send(new JoinGlobalMissionCommand(id, _currentUser.UserId));

        if (!response.Success)
        {
            if (response.Message == "Görev bulunamadı.")
                return NotFound(response);
            
            if (response.Message == "Bu görev aktif değil.")
                return BadRequest(response);

            // Zaten katılmışsa Ok dön
            if (response.Message == "Zaten bu göreve katılmışsınız.")
                return Ok(response);
            
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Global görev detayı.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GlobalMissionDetailDto>> GetMissionDetail(Guid id)
    {
        var response = await _mediator.Send(new GetGlobalMissionDetailQuery(id, _currentUser.UserId));

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    /// <summary>
    /// Geçmiş global görevler.
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ActiveGlobalMissionsResponse>> GetMissionHistory(int limit = 10)
    {
        // Uses same response type as Active
        var response = await _mediator.Send(new GetGlobalMissionHistoryQuery(_currentUser.UserId, limit));
        return Ok(response);
    }
}
