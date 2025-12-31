using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Social.Application.Commands;

public sealed record UnfollowUserCommand(Guid UserId, Guid TargetUserId) : IRequest<FollowResponse>;

public sealed class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, FollowResponse>
{
    private readonly IFriendshipRepository _friendshipRepo;
    private readonly ISocialUserRepository _socialUserRepo;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly ILogger<UnfollowUserCommandHandler> _logger;

    public UnfollowUserCommandHandler(
        IFriendshipRepository friendshipRepo,
        ISocialUserRepository socialUserRepo,
        ISocialUnitOfWork unitOfWork,
        ILogger<UnfollowUserCommandHandler> logger)
    {
        _friendshipRepo = friendshipRepo;
        _socialUserRepo = socialUserRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FollowResponse> Handle(UnfollowUserCommand request, CancellationToken ct)
    {
        var friendship = await _friendshipRepo.GetAsync(request.UserId, request.TargetUserId, ct);
        if (friendship == null)
        {
             return new FollowResponse { 
                 Success = true, 
                 Message = "Takip etmiyordunuz.",
                 FollowingCount = await _friendshipRepo.CountFollowingAsync(request.UserId, ct),
                 IsMutual = false
             };
        }

        try
        {
            await _friendshipRepo.RemoveAsync(friendship, ct);

            await _socialUserRepo.DecrementFollowingAsync(request.UserId, ct);
            await _socialUserRepo.DecrementFollowersAsync(request.TargetUserId, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            var followingCount = await _friendshipRepo.CountFollowingAsync(request.UserId, ct);

            return new FollowResponse
            {
                Success = true,
                Message = "Takip bırakıldı.",
                FollowingCount = followingCount,
                IsMutual = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfollowing user {TargetId}", request.TargetUserId);
            return new FollowResponse { Success = false, Message = "Bir hata oluştu." };
        }
    }
}
