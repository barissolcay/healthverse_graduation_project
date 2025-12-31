using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Competition.Application.Services;

/// <summary>
/// Haftalık lig finalize işlemleri servisi.
/// 
/// Bu servis Quartz.NET job tarafından çağrılır.
/// Her Pazar 23:59 TR'de çalışır.
/// </summary>
public class LeagueFinalizeService
{
    private readonly ILeagueRoomRepository _roomRepository;
    private readonly ILeagueMemberRepository _memberRepository;
    private readonly ILeagueConfigRepository _configRepository;
    private readonly ICompetitionUserRepository _userRepository;
    private readonly IUserPointsHistoryRepository _historyRepository;
    private readonly ICompetitionUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<LeagueFinalizeService> _logger;

    public LeagueFinalizeService(
        ILeagueRoomRepository roomRepository,
        ILeagueMemberRepository memberRepository,
        ILeagueConfigRepository configRepository,
        ICompetitionUserRepository userRepository,
        IUserPointsHistoryRepository historyRepository,
        ICompetitionUnitOfWork unitOfWork,
        IClock clock,
        ILogger<LeagueFinalizeService> logger)
    {
        _roomRepository = roomRepository;
        _memberRepository = memberRepository;
        _configRepository = configRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen haftanın tüm odalarını finalize eder.
    /// </summary>
    public async Task<FinalizeResult> FinalizeWeek(string weekIdValue)
    {
        _logger.LogInformation("League finalize başlıyor. WeekId: {WeekId}", weekIdValue);

        var result = new FinalizeResult { WeekId = weekIdValue };

        var weekId = WeekId.Create(weekIdValue);
        var rooms = await _roomRepository.GetUnprocessedByWeekAsync(weekId);

        _logger.LogInformation("{RoomCount} adet oda işlenecek.", rooms.Count);
        result.TotalRooms = rooms.Count;

        foreach (var room in rooms)
        {
            try
            {
                await ProcessRoom(room, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oda işlenirken hata. RoomId: {RoomId}", room.Id);
                result.FailedRooms++;
            }
        }

        result.ProcessedRooms = result.TotalRooms - result.FailedRooms;
        _logger.LogInformation("League finalize tamamlandı. İşlenen: {Processed}, Başarısız: {Failed}",
            result.ProcessedRooms, result.FailedRooms);

        return result;
    }

    private async Task ProcessRoom(LeagueRoom room, FinalizeResult result)
    {
        var tierConfig = await _configRepository.GetByTierNameAsync(room.Tier);

        if (tierConfig == null)
        {
            _logger.LogWarning("Tier config bulunamadı: {Tier}", room.Tier);
            return;
        }

        var members = await _memberRepository.GetMembersByRoomOrderedAsync(room.Id);

        var totalMembers = members.Count;
        if (totalMembers == 0)
        {
            _logger.LogInformation("Oda boş, atlanıyor. RoomId: {RoomId}", room.Id);
            room.TryMarkAsProcessed(_clock.UtcNow);
            await _unitOfWork.SaveChangesAsync();
            return;
        }

        var promoteCount = (int)Math.Ceiling(totalMembers * tierConfig.PromotePercentage / 100.0);
        var demoteCount = (int)Math.Floor(totalMembers * tierConfig.DemotePercentage / 100.0);

        var nextTier = await _configRepository.GetNextByOrderAsync(tierConfig.TierOrder);
        var prevTier = await _configRepository.GetPrevByOrderAsync(tierConfig.TierOrder);

        var rank = 0;
        foreach (var member in members)
        {
            rank++;
            member.UpdateRank(rank);

            var user = await _userRepository.GetByIdAsync(member.UserId);
            if (user == null) continue;

            string resultType;
            string newTier = room.Tier;

            if (rank <= promoteCount && nextTier != null)
            {
                resultType = "PROMOTED";
                newTier = nextTier.TierName;
                user.UpdateTier(newTier);
                result.PromotedUsers++;
            }
            else if (rank > totalMembers - demoteCount && prevTier != null)
            {
                resultType = "DEMOTED";
                newTier = prevTier.TierName;
                user.UpdateTier(newTier);
                result.DemotedUsers++;
            }
            else
            {
                resultType = "STAYED";
                result.StayedUsers++;
            }

            var history = UserPointsHistory.CreateWeekly(
                member.UserId,
                room.WeekId.Value,
                member.PointsInRoom,
                rank,
                room.Tier,
                resultType
            );
            await _historyRepository.AddWeeklyHistoryAsync(history);
        }

        room.TryMarkAsProcessed(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Oda işlendi. RoomId: {RoomId}, Üye: {Members}, Promote: {Promote}, Demote: {Demote}",
            room.Id, totalMembers, promoteCount, demoteCount);
    }
}

/// <summary>
/// Finalize işlemi sonucu.
/// </summary>
public class FinalizeResult
{
    public string WeekId { get; set; } = null!;
    public int TotalRooms { get; set; }
    public int ProcessedRooms { get; set; }
    public int FailedRooms { get; set; }
    public int PromotedUsers { get; set; }
    public int DemotedUsers { get; set; }
    public int StayedUsers { get; set; }
}
