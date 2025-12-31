using HealthVerse.Contracts.Notifications;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Social.Application.Commands;

public sealed record FollowUserCommand(Guid UserId, Guid TargetUserId) : IRequest<FollowResponse>;

public sealed class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, FollowResponse>
{
    private readonly IFriendshipRepository _friendshipRepo;
    private readonly IUserBlockRepository _blockRepo;
    private readonly ISocialUserRepository _socialUserRepo;
    private readonly INotificationService _notificationService;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly ILogger<FollowUserCommandHandler> _logger;

    public FollowUserCommandHandler(
        IFriendshipRepository friendshipRepo,
        IUserBlockRepository blockRepo,
        ISocialUserRepository socialUserRepo,
        INotificationService notificationService,
        ISocialUnitOfWork unitOfWork,
        ILogger<FollowUserCommandHandler> logger)
    {
        _friendshipRepo = friendshipRepo;
        _blockRepo = blockRepo;
        _socialUserRepo = socialUserRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FollowResponse> Handle(FollowUserCommand request, CancellationToken ct)
    {
        if (request.UserId == request.TargetUserId)
        {
            return new FollowResponse { Success = false, Message = "Kendinizi takip edemezsiniz." };
        }

        // 1. Check Block
        if (await _blockRepo.IsBlockedEitherWayAsync(request.UserId, request.TargetUserId, ct))
        {
            return new FollowResponse { Success = false, Message = "İşlem gerçekleştirilemedi." };
        }

        // 2. Check Existing
        if (await _friendshipRepo.ExistsAsync(request.UserId, request.TargetUserId, ct))
        {
            return new FollowResponse { Success = true, Message = "Zaten takip ediyorsunuz." };
        }

        try
        {
            // 3. Add Friendship
            var friendship = Friendship.Create(request.UserId, request.TargetUserId);
            await _friendshipRepo.AddAsync(friendship, ct);

            // 4. Update Counters
            await _socialUserRepo.IncrementFollowingAsync(request.UserId, ct);
            await _socialUserRepo.IncrementFollowersAsync(request.TargetUserId, ct);

            // 5. Check Mutual (Is the target following me?)
            // After adding the current follow, mutual = target already follows requester
            bool isMutual = await _friendshipRepo.ExistsAsync(request.TargetUserId, request.UserId, ct);

            // 6. Notifications
            if (isMutual)
            {
                // Both are friends now - send to both
                await _notificationService.CreateBatchAsync(new[]
                {
                    new NotificationCreateRequest(
                        request.UserId,
                        NotificationType.MUTUAL_FRIEND,
                        "Yeni Arkadaş! 🎉",
                        "Artık arkadaşsınız!",
                        request.TargetUserId,
                        "USER"),
                    new NotificationCreateRequest(
                        request.TargetUserId,
                        NotificationType.MUTUAL_FRIEND,
                        "Yeni Arkadaş! 🎉",
                        "Artık arkadaşsınız!",
                        request.UserId,
                        "USER")
                }, ct);
            }
            else
            {
                // Just follower
                await _notificationService.CreateAsync(
                    request.TargetUserId,
                    NotificationType.NEW_FOLLOWER,
                    "Yeni Takipçi 👋",
                    "Seni takip etmeye başladı!",
                    request.UserId,
                    "USER",
                    null,
                    ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var followingCount = await _friendshipRepo.CountFollowingAsync(request.UserId, ct);

            return new FollowResponse
            {
                Success = true,
                Message = "Takip edildi.",
                FollowingCount = followingCount,
                IsMutual = isMutual
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error following user {TargetId}", request.TargetUserId);
            return new FollowResponse { Success = false, Message = "Bir hata oluştu." };
        }
    }
}
