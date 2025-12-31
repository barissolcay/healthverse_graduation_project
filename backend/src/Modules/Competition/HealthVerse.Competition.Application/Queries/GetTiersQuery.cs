using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Competition.Application.Ports;
using MediatR;

namespace HealthVerse.Competition.Application.Queries;

public sealed record GetTiersQuery(Guid UserId) : IRequest<TiersResponse>;

public sealed class GetTiersQueryHandler : IRequestHandler<GetTiersQuery, TiersResponse>
{
    private readonly ILeagueConfigRepository _configRepository;
    private readonly ICompetitionUserRepository _userRepository;

    public GetTiersQueryHandler(
        ILeagueConfigRepository configRepository,
        ICompetitionUserRepository userRepository)
    {
        _configRepository = configRepository;
        _userRepository = userRepository;
    }

    public async Task<TiersResponse> Handle(GetTiersQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        var userTier = user?.CurrentTier ?? "ISINMA";

        var configs = await _configRepository.GetAllOrderedAsync(ct);
        var tiers = configs.Select(c => new TierInfoDto
        {
            TierName = c.TierName,
            TierOrder = c.TierOrder,
            PromotePercentage = c.PromotePercentage,
            DemotePercentage = c.DemotePercentage,
            MinRoomSize = c.MinRoomSize,
            MaxRoomSize = c.MaxRoomSize,
            IsLowestTier = c.DemotePercentage == 0,
            IsHighestTier = c.PromotePercentage == 0
        }).ToList();

        return new TiersResponse
        {
            Tiers = tiers,
            UserCurrentTier = userTier
        };
    }
}
