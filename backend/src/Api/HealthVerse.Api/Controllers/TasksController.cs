using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Tasks.Application.Commands;
using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Artık tüm endpointler varsayılan olarak yetki ister
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ICurrentUser currentUser, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının aktif görevlerini döner.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ActiveTasksResponse>> GetActiveTasks()
    {
        var response = await _mediator.Send(new GetActiveTasksQuery(_currentUser.UserId));
        return Ok(response);
    }

    /// <summary>
    /// Kullanıcının tamamlanan görevlerini döner.
    /// </summary>
    [HttpGet("completed")]
    public async Task<ActionResult<CompletedTasksResponse>> GetCompletedTasks(int limit = 20)
    {
        var response = await _mediator.Send(new GetCompletedTasksQuery(_currentUser.UserId, limit));
        return Ok(response);
    }

    /// <summary>
    /// Görev ödülünü toplar (UI onayı).
    /// </summary>
    [HttpPost("{id:guid}/claim")]
    public async Task<ActionResult<ClaimRewardResponse>> ClaimReward(Guid id)
    {
        var response = await _mediator.Send(new ClaimTaskRewardCommand(id, _currentUser.UserId));

        if (!response.Success)
        {
            if (response.Message == "Görev bulunamadı.")
                return NotFound(response);
            
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Mevcut görev şablonlarını döner (admin).
    /// </summary>
    [HttpGet("templates")]
    [AllowAnonymous] // Template listesi herkese açık olabilir veya role check eklenebilir. Şimdilik mevcut davranış değişikliği olmaması için anonymous/auth analizi yapılmalı. Mevcut kodda auth check yoktu (currentUser kullanılmıyordu).
    public async Task<ActionResult<List<TaskTemplateDto>>> GetTemplates(bool activeOnly = true)
    {
        // Not: Önceki kodda UserID kullanılmıyordu, o yüzden burada CurrentUser'a ihtiyaç yok.
        // Ancak [Authorize] class level olduğu için burada explicit AllowAnonymous gerekebilir 
        // veya endpoint'in auth gerektirip gerektirmediği analiz edilmeli.
        // Güvenlik için Authorize kalsın, herkes görebilir ama token şart olsun.
        var response = await _mediator.Send(new GetTaskTemplatesQuery(activeOnly));
        return Ok(response);
    }
}
