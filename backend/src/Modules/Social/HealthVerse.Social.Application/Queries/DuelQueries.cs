using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Domain.Entities;
using MediatR;

namespace HealthVerse.Social.Application.Queries;

// --- Queries ---

public sealed record GetPendingDuelsQuery(Guid UserId) : IRequest<PendingDuelsResponse>;
public sealed record GetActiveDuelsQuery(Guid UserId) : IRequest<ActiveDuelsResponse>;
public sealed record GetDuelHistoryQuery(Guid UserId, int Limit = 10) : IRequest<DuelHistoryResponse>;
public sealed record GetDuelDetailQuery(Guid DuelId, Guid UserId) : IRequest<DuelDetailDto?>;

// --- Handlers ---

public sealed class DuelQueryHandlers :
    IRequestHandler<GetPendingDuelsQuery, PendingDuelsResponse>,
    IRequestHandler<GetActiveDuelsQuery, ActiveDuelsResponse>,
    IRequestHandler<GetDuelHistoryQuery, DuelHistoryResponse>,
    IRequestHandler<GetDuelDetailQuery, DuelDetailDto?>
{
    private readonly IDuelRepository _duelRepo;
    private readonly ISocialUserRepository _userRepo;
    private readonly IClock _clock;

    public DuelQueryHandlers(
        IDuelRepository duelRepo, 
        ISocialUserRepository userRepo,
        IClock clock)
    {
        _duelRepo = duelRepo;
        _userRepo = userRepo;
        _clock = clock;
    }

    // --- GetPendingDuels ---
    public async Task<PendingDuelsResponse> Handle(GetPendingDuelsQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        // Expire first
        await _duelRepo.ExpireOldDuelsAsync(now, ct);

        var incoming = await _duelRepo.GetPendingIncomingAsync(request.UserId, ct);
        var outgoing = await _duelRepo.GetPendingOutgoingAsync(request.UserId, ct);

        var allDuels = incoming.Concat(outgoing).ToList();
        var profiles = await GetProfilesAsync(allDuels, ct);

        return new PendingDuelsResponse
        {
            Incoming = incoming.Select(d => MapToDto(d, request.UserId, profiles, now)).ToList(),
            Outgoing = outgoing.Select(d => MapToDto(d, request.UserId, profiles, now)).ToList(),
            TotalPending = incoming.Count + outgoing.Count
        };
    }

    // --- GetActiveDuels ---
    public async Task<ActiveDuelsResponse> Handle(GetActiveDuelsQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        // Finish expired
        await _duelRepo.FinishExpiredDuelsAsync(now, ct);

        var activeDuels = await _duelRepo.GetActiveByUserAsync(request.UserId, ct);
        var profiles = await GetProfilesAsync(activeDuels, ct);

        return new ActiveDuelsResponse
        {
            Duels = activeDuels.Select(d => MapToDto(d, request.UserId, profiles, now)).ToList(),
            TotalActive = activeDuels.Count
        };
    }

    // --- GetDuelHistory ---
    public async Task<DuelHistoryResponse> Handle(GetDuelHistoryQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var limit = Math.Max(1, Math.Min(request.Limit, 20));
        var history = await _duelRepo.GetHistoryByUserAsync(request.UserId, limit, ct);
        var profiles = await GetProfilesAsync(history, ct);

        return new DuelHistoryResponse
        {
            Duels = history.Select(d => MapToDto(d, request.UserId, profiles, now)).ToList(),
            TotalCompleted = history.Count
        };
    }

    // --- GetDuelDetail ---
    public async Task<DuelDetailDto?> Handle(GetDuelDetailQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var duel = await _duelRepo.GetByIdAsync(request.DuelId, ct);
        if (duel == null) return null;

        var profiles = await GetProfilesAsync(new[] { duel }, ct);
        return MapToDto(duel, request.UserId, profiles, now);
    }

    // --- Helpers ---

    private async Task<Dictionary<Guid, (string Name, int Avatar)>> GetProfilesAsync(IEnumerable<Duel> duels, CancellationToken ct)
    {
        var userIds = duels.SelectMany(d => new[] { d.ChallengerId, d.OpponentId }).Distinct().ToList();
        
        var profiles = await _userRepo.GetProfilesAsync(userIds, ct);

        var result = new Dictionary<Guid, (string, int)>();
        foreach(var uid in userIds)
        {
            if (profiles.TryGetValue(uid, out var summary))
            {
                result[uid] = (summary.Username, summary.AvatarId);
            }
            else
            {
                result[uid] = ("Bilinmeyen", 1);
            }
        }
        return result;
    }

    private DuelDetailDto MapToDto(Duel d, Guid currentUserId, Dictionary<Guid, (string Name, int Avatar)> profiles, DateTimeOffset now)
    {
        profiles.TryGetValue(d.ChallengerId, out var challenger);
        profiles.TryGetValue(d.OpponentId, out var opponent);

        int cPercent = d.TargetValue > 0 ? (int)((double)d.ChallengerScore / d.TargetValue * 100) : 0;
        int oPercent = d.TargetValue > 0 ? (int)((double)d.OpponentScore / d.TargetValue * 100) : 0;

        int? hoursRemaining = null;
        if (d.Status == DuelStatus.ACTIVE && d.EndDate.HasValue)
        {
            hoursRemaining = (int)Math.Max(0, (d.EndDate.Value - now).TotalHours);
        }

        bool isChallenger = currentUserId == d.ChallengerId;
        bool canPoke = false;
        
        if (d.Status == DuelStatus.ACTIVE)
        {
             if (isChallenger)
             {
                 // Can poke if not poked today
                 canPoke = d.ChallengerLastPokeAt == null || d.ChallengerLastPokeAt.Value.Date != now.Date;
             }
             else
             {
                 canPoke = d.OpponentLastPokeAt == null || d.OpponentLastPokeAt.Value.Date != now.Date;
             }
        }

        return new DuelDetailDto
        {
            Id = d.Id,
            ChallengerId = d.ChallengerId,
            ChallengerName = challenger.Name ?? "Kullanıcı",
            ChallengerAvatarId = challenger.Avatar == 0 ? 1 : challenger.Avatar,
            OpponentId = d.OpponentId,
            OpponentName = opponent.Name ?? "Kullanıcı",
            OpponentAvatarId = opponent.Avatar == 0 ? 1 : opponent.Avatar,
            ActivityType = d.ActivityType,
            TargetMetric = d.TargetMetric,
            TargetValue = d.TargetValue,
            DurationDays = d.DurationDays,
            Status = d.Status,
            ChallengerScore = d.ChallengerScore,
            OpponentScore = d.OpponentScore,
            ChallengerProgressPercent = Math.Min(100, cPercent),
            OpponentProgressPercent = Math.Min(100, oPercent),
            Result = d.Result,
            StartDate = d.StartDate,
            EndDate = d.EndDate,
            HoursRemaining = hoursRemaining,
            CreatedAt = d.CreatedAt,
            IsChallenger = isChallenger,
            CanPoke = canPoke
        };
    }
}
