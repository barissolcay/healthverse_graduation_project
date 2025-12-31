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
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<GoalsController> _logger;

    public GoalsController(IMediator mediator, ICurrentUser currentUser, ILogger<GoalsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Yeni hedef oluşturur.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateGoalResponse>> CreateGoal([FromBody] CreateGoalRequest request)
    {
        var command = new CreateGoalCommand(
            _currentUser.UserId,
            request.Title,
            request.TargetMetric,
            request.TargetValue,
            request.ValidUntil,
            request.Description,
            request.ActivityType
        );

        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Kullanıcının aktif hedeflerini döner.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ActiveGoalsResponse>> GetActiveGoals()
    {
        var response = await _mediator.Send(new GetActiveGoalsQuery(_currentUser.UserId));
        return Ok(response);
    }

    /// <summary>
    /// Kullanıcının tamamlanan hedeflerini döner.
    /// </summary>
    [HttpGet("completed")]
    public async Task<ActionResult<CompletedGoalsResponse>> GetCompletedGoals(int limit = 20)
    {
        var response = await _mediator.Send(new GetCompletedGoalsQuery(_currentUser.UserId, limit));
        return Ok(response);
    }

    /// <summary>
    /// Aktif hedefi siler.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteGoal(Guid id)
    {
        var response = await _mediator.Send(new DeleteGoalCommand(id, _currentUser.UserId));

        if (!response.Success)
        {
            if (response.Message == "Hedef bulunamadı.")
                return NotFound(new { Message = response.Message });
            
            return BadRequest(new { Message = response.Message });
        }

        return Ok(new { Message = response.Message });
    }
}
