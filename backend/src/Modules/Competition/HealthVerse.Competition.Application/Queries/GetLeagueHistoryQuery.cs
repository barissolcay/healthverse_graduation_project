using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Competition.Application.Ports;
using MediatR;

namespace HealthVerse.Competition.Application.Queries;

public sealed record GetLeagueHistoryQuery(Guid UserId, int Limit) : IRequest<LeagueHistoryResponse>;

public sealed class GetLeagueHistoryQueryHandler : IRequestHandler<GetLeagueHistoryQuery, LeagueHistoryResponse>
{
    private readonly IUserPointsHistoryRepository _historyRepository;

    public GetLeagueHistoryQueryHandler(IUserPointsHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<LeagueHistoryResponse> Handle(GetLeagueHistoryQuery request, CancellationToken ct)
    {
        var limit = Math.Min(Math.Max(request.Limit, 1), 52);
        var records = await _historyRepository.GetWeeklyHistoryAsync(request.UserId, limit, ct);

        var history = records.Select(h => new LeagueHistoryItem
        {
            WeekId = h.PeriodId,
            Tier = h.TierAtTime ?? "ISINMA",
            Points = h.Points,
            Rank = h.LeagueRank,
            Result = h.Result ?? "STAYED"
        }).ToList();

        return new LeagueHistoryResponse
        {
            History = history,
            TotalWeeks = history.Count
        };
    }
}
