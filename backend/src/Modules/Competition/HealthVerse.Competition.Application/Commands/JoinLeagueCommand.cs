using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Competition.Application.Commands;

public sealed record JoinLeagueCommand(Guid UserId) : IRequest<JoinLeagueResponse>;

public sealed class JoinLeagueCommandHandler : IRequestHandler<JoinLeagueCommand, JoinLeagueResponse>
{
    private readonly ICompetitionUserRepository _userRepository;
    private readonly ILeagueConfigRepository _configRepository;
    private readonly ILeagueRoomRepository _roomRepository;
    private readonly ILeagueMemberRepository _memberRepository;
    private readonly ICompetitionUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<JoinLeagueCommandHandler> _logger;

    public JoinLeagueCommandHandler(
        ICompetitionUserRepository userRepository,
        ILeagueConfigRepository configRepository,
        ILeagueRoomRepository roomRepository,
        ILeagueMemberRepository memberRepository,
        ICompetitionUnitOfWork unitOfWork,
        IClock clock,
        ILogger<JoinLeagueCommandHandler> logger)
    {
        _userRepository = userRepository;
        _configRepository = configRepository;
        _roomRepository = roomRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<JoinLeagueResponse> Handle(JoinLeagueCommand request, CancellationToken ct)
    {
        var currentWeekId = WeekId.FromDate(_clock.TodayTR);

        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return new JoinLeagueResponse { Success = false, Message = "Kullanıcı bulunamadı." };
        }

        var existingMembership = await _memberRepository.GetMembershipByUserAndWeekAsync(request.UserId, currentWeekId, ct);
        if (existingMembership is not null)
        {
            var existingRoom = await _roomRepository.GetByIdAsync(existingMembership.RoomId, ct);
            return new JoinLeagueResponse
            {
                Success = true,
                Message = "Zaten bu hafta bir odadasınız.",
                RoomId = existingMembership.RoomId,
                WeekId = currentWeekId.Value,
                Tier = existingRoom?.Tier ?? user.CurrentTier
            };
        }

        var userTier = user.CurrentTier;
        var tierConfig = await _configRepository.GetByTierNameAsync(userTier, ct);

        if (tierConfig is null)
        {
            userTier = "ISINMA";
            tierConfig = await _configRepository.GetByTierNameAsync("ISINMA", ct);
        }

        var maxRoomSize = tierConfig?.MaxRoomSize ?? 20;
        var availableRoom = await _roomRepository.FindAvailableForTierAsync(currentWeekId, userTier, maxRoomSize, ct);

        if (availableRoom is null)
        {
            var weekStart = GetWeekStartDate(_clock.TodayTR);
            var weekEnd = weekStart.AddDays(7);

            availableRoom = LeagueRoom.Create(
                currentWeekId,
                userTier,
                new DateTimeOffset(weekStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                new DateTimeOffset(weekEnd.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            );

            await _roomRepository.AddAsync(availableRoom, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var member = LeagueMember.Create(availableRoom.Id, currentWeekId, request.UserId);
        await _memberRepository.AddAsync(member, ct);
        await _roomRepository.IncrementUserCountAsync(availableRoom.Id, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} joined league room {RoomId} in tier {Tier}",
            request.UserId, availableRoom.Id, userTier);

        return new JoinLeagueResponse
        {
            Success = true,
            Message = $"Lige katıldınız! Tier: {userTier}",
            RoomId = availableRoom.Id,
            WeekId = currentWeekId.Value,
            Tier = userTier
        };
    }

    private static DateOnly GetWeekStartDate(DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return date.AddDays(-daysToMonday);
    }
}
