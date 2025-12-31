using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using MediatR;

namespace HealthVerse.Tasks.Application.Queries;

public sealed record GetActiveTasksQuery(Guid UserId) : IRequest<ActiveTasksResponse>;

public sealed class GetActiveTasksQueryHandler : IRequestHandler<GetActiveTasksQuery, ActiveTasksResponse>
{
    private readonly IUserTaskRepository _userTaskRepository;
    private readonly ITaskTemplateRepository _taskTemplateRepository;
    private readonly ITasksUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public GetActiveTasksQueryHandler(
        IUserTaskRepository userTaskRepository,
        ITaskTemplateRepository taskTemplateRepository,
        ITasksUnitOfWork unitOfWork,
        IClock clock)
    {
        _userTaskRepository = userTaskRepository;
        _taskTemplateRepository = taskTemplateRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<ActiveTasksResponse> Handle(GetActiveTasksQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        // 1. Mark expired tasks as FAILED
        // Note: Ideally this should be done by the ExpireJob, but we keep "expire on load" 
        // as a fallback/immediate consistency mechanism for MVP.
        var expiredTasks = await _userTaskRepository.GetExpiredAsync(request.UserId, now, ct);
        foreach (var task in expiredTasks)
        {
            task.MarkAsFailed();
        }

        if (expiredTasks.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // 2. Fetch active tasks
        var activeTasks = await _userTaskRepository.GetActiveByUserAsync(request.UserId, ct);

        if (activeTasks.Count == 0)
        {
            return new ActiveTasksResponse
            {
                Tasks = new List<TaskDetailDto>(),
                TotalActive = 0
            };
        }

        // 3. Fetch templates
        var templateIds = activeTasks.Select(t => t.TemplateId).Distinct();
        var templates = await _taskTemplateRepository.GetByIdsAsync(templateIds, ct);

        // 4. Map to DTOs
        var taskDtos = activeTasks
            .Select(t => MapToTaskDetailDto(t, templates.GetValueOrDefault(t.TemplateId), now))
            .OrderBy(t => t.ValidUntil)
            .ToList();

        return new ActiveTasksResponse
        {
            Tasks = taskDtos,
            TotalActive = taskDtos.Count
        };
    }

    private static TaskDetailDto MapToTaskDetailDto(UserTask task, TaskTemplate? template, DateTimeOffset now)
    {
        var targetValue = template?.TargetValue ?? 1;
        var progressPercent = targetValue > 0 
            ? Math.Min(100, (int)((task.CurrentValue / (double)targetValue) * 100)) 
            : 0;
        var hoursRemaining = (int)Math.Max(0, (task.ValidUntil - now).TotalHours);

        return new TaskDetailDto
        {
            Id = task.Id,
            TemplateId = task.TemplateId,
            Title = template?.Title ?? "Görev",
            Description = template?.Description,
            Category = template?.Category,
            ActivityType = template?.ActivityType,
            TargetMetric = template?.TargetMetric ?? "STEPS",
            TargetValue = template?.TargetValue ?? 0,
            RewardPoints = template?.RewardPoints ?? 0,
            BadgeId = template?.BadgeId,
            TitleId = template?.TitleId,
            CurrentValue = task.CurrentValue,
            ProgressPercent = progressPercent,
            Status = task.Status,
            ValidUntil = task.ValidUntil,
            AssignedAt = task.AssignedAt,
            CompletedAt = task.CompletedAt,
            RewardClaimedAt = task.RewardClaimedAt,
            HoursRemaining = hoursRemaining
        };
    }
}
