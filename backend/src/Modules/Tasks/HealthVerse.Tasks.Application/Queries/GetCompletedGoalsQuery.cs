using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using MediatR;

namespace HealthVerse.Tasks.Application.Queries;

public sealed record GetCompletedGoalsQuery(Guid UserId, int Limit = 20) : IRequest<CompletedGoalsResponse>;

public sealed class GetCompletedGoalsQueryHandler : IRequestHandler<GetCompletedGoalsQuery, CompletedGoalsResponse>
{
    private readonly IUserGoalRepository _repository;
    private readonly IClock _clock;

    public GetCompletedGoalsQueryHandler(IUserGoalRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<CompletedGoalsResponse> Handle(GetCompletedGoalsQuery request, CancellationToken ct)
    {
        var limit = Math.Min(Math.Max(request.Limit, 1), 50);
        var goals = await _repository.GetCompletedByUserAsync(request.UserId, limit, ct);
        var now = _clock.UtcNow;

        var goalDtos = goals.Select(g => new GoalDetailDto
        {
            Id = g.Id,
            Title = g.Title,
            Description = g.Description,
            ActivityType = g.ActivityType,
            TargetMetric = g.TargetMetric,
            TargetValue = g.TargetValue,
            CurrentValue = g.CurrentValue,
            ProgressPercent = 100, // Completed
            ValidUntil = g.ValidUntil,
            CreatedAt = g.CreatedAt,
            CompletedAt = g.CompletedAt,
            HoursRemaining = 0,
            IsCompleted = true
        }).ToList();

        return new CompletedGoalsResponse
        {
            Goals = goalDtos,
            TotalCompleted = goalDtos.Count
        };
    }
}
