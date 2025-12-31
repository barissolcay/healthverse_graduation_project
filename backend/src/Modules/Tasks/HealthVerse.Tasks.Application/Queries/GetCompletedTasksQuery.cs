using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using MediatR;

namespace HealthVerse.Tasks.Application.Queries;

public sealed record GetCompletedTasksQuery(Guid UserId, int Limit = 20) : IRequest<CompletedTasksResponse>;

public sealed class GetCompletedTasksQueryHandler : IRequestHandler<GetCompletedTasksQuery, CompletedTasksResponse>
{
    private readonly IUserTaskRepository _userTaskRepository;
    private readonly ITaskTemplateRepository _taskTemplateRepository;
    private readonly IClock _clock;

    public GetCompletedTasksQueryHandler(
        IUserTaskRepository userTaskRepository,
        ITaskTemplateRepository taskTemplateRepository,
        IClock clock)
    {
        _userTaskRepository = userTaskRepository;
        _taskTemplateRepository = taskTemplateRepository;
        _clock = clock;
    }

    public async Task<CompletedTasksResponse> Handle(GetCompletedTasksQuery request, CancellationToken ct)
    {
        var limit = Math.Min(Math.Max(request.Limit, 1), 50);
        var completedTasks = await _userTaskRepository.GetCompletedByUserAsync(request.UserId, limit, ct);

        if (completedTasks.Count == 0)
        {
            return new CompletedTasksResponse
            {
                Tasks = new List<TaskDetailDto>(),
                TotalCompleted = 0
            };
        }

        var templateIds = completedTasks.Select(t => t.TemplateId).Distinct();
        var templates = await _taskTemplateRepository.GetByIdsAsync(templateIds, ct);
        var now = _clock.UtcNow;

        var taskDtos = completedTasks
            .Select(t => MapToTaskDetailDto(t, templates.GetValueOrDefault(t.TemplateId), now))
            .ToList();

        return new CompletedTasksResponse
        {
            Tasks = taskDtos,
            TotalCompleted = taskDtos.Count
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
