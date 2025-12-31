using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using MediatR;

namespace HealthVerse.Tasks.Application.Queries;

public sealed record GetActiveGoalsQuery(Guid UserId) : IRequest<ActiveGoalsResponse>;

public sealed class GetActiveGoalsQueryHandler : IRequestHandler<GetActiveGoalsQuery, ActiveGoalsResponse>
{
    private readonly IUserGoalRepository _repository;
    private readonly IClock _clock;

    public GetActiveGoalsQueryHandler(IUserGoalRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ActiveGoalsResponse> Handle(GetActiveGoalsQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var goals = await _repository.GetActiveByUserAsync(request.UserId, now, ct);

        var goalDtos = goals.Select(g => new GoalDetailDto
        {
            Id = g.Id,
            Title = g.Title,
            Description = g.Description,
            ActivityType = g.ActivityType,
            TargetMetric = g.TargetMetric,
            TargetValue = g.TargetValue,
            CurrentValue = g.CurrentValue,
            ProgressPercent = g.TargetValue > 0 ? Math.Min(100, (int)((g.CurrentValue / (double)g.TargetValue) * 100)) : 0,
            ValidUntil = g.ValidUntil,
            CreatedAt = g.CreatedAt,
            CompletedAt = g.CompletedAt,
            HoursRemaining = (int)Math.Max(0, (g.ValidUntil - now).TotalHours),
            IsCompleted = g.CurrentValue >= g.TargetValue
        }).ToList();

        return new ActiveGoalsResponse
        {
            Goals = goalDtos,
            TotalActive = goalDtos.Count
        };
    }
}
