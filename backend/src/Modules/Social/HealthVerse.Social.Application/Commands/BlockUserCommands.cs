using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Social.Application.Commands;

// --- Block Command ---
public sealed record BlockUserCommand(Guid UserId, Guid TargetUserId) : IRequest<BlockResponse>;

public sealed class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, BlockResponse>
{
    private readonly IUserBlockRepository _blockRepo;
    private readonly IFriendshipRepository _friendshipRepo;
    private readonly ISocialUserRepository _socialUserRepo;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly ILogger<BlockUserCommandHandler> _logger;

    public BlockUserCommandHandler(
        IUserBlockRepository blockRepo,
        IFriendshipRepository friendshipRepo,
        ISocialUserRepository socialUserRepo,
        ISocialUnitOfWork unitOfWork,
        ILogger<BlockUserCommandHandler> logger)
    {
        _blockRepo = blockRepo;
        _friendshipRepo = friendshipRepo;
        _socialUserRepo = socialUserRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BlockResponse> Handle(BlockUserCommand request, CancellationToken ct)
    {
        if (request.UserId == request.TargetUserId)
        {
            return new BlockResponse { Success = false, Message = "Kendinizi engelleyemezsiniz." };
        }

        // Already blocked by YOU?
        var existingBlock = await _blockRepo.GetAsync(request.UserId, request.TargetUserId, ct);
        if (existingBlock != null)
        {
            return new BlockResponse { Success = true, Message = "Kullanıcı zaten engelli." };
        }

        try
        {
            // 1. Create Block
            var block = UserBlock.Create(request.UserId, request.TargetUserId);
            await _blockRepo.AddAsync(block, ct);

            // 2. Remove Friendships (if any)
            // A -> B (Me following Target)
            var f1 = await _friendshipRepo.GetAsync(request.UserId, request.TargetUserId, ct);
            if (f1 != null)
            {
                await _friendshipRepo.RemoveAsync(f1, ct);
                await _socialUserRepo.DecrementFollowingAsync(request.UserId, ct);
                await _socialUserRepo.DecrementFollowersAsync(request.TargetUserId, ct);
            }

            // B -> A (Target following Me)
            var f2 = await _friendshipRepo.GetAsync(request.TargetUserId, request.UserId, ct);
            if (f2 != null)
            {
                await _friendshipRepo.RemoveAsync(f2, ct);
                await _socialUserRepo.DecrementFollowingAsync(request.TargetUserId, ct);
                await _socialUserRepo.DecrementFollowersAsync(request.UserId, ct);
            }

            // Note: Duels removal logic is currently out of scope here as per plan, 
            // but effectively blocked users can't interact. 
            // Existing duels might remain active until expiry or manual cancellation, 
            // but future interactions are blocked.

            await _unitOfWork.SaveChangesAsync(ct);

            return new BlockResponse
            {
                Success = true,
                Message = "Kullanıcı engellendi."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {TargetId}", request.TargetUserId);
            return new BlockResponse { Success = false, Message = "Bir hata oluştu." };
        }
    }
}

// --- Unblock Command ---
public sealed record UnblockUserCommand(Guid UserId, Guid TargetUserId) : IRequest<BlockResponse>;

public sealed class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, BlockResponse>
{
    private readonly IUserBlockRepository _blockRepo;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly ILogger<UnblockUserCommandHandler> _logger;

    public UnblockUserCommandHandler(
        IUserBlockRepository blockRepo,
        ISocialUnitOfWork unitOfWork,
        ILogger<UnblockUserCommandHandler> logger)
    {
        _blockRepo = blockRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BlockResponse> Handle(UnblockUserCommand request, CancellationToken ct)
    {
        var existingBlock = await _blockRepo.GetAsync(request.UserId, request.TargetUserId, ct);
        if (existingBlock == null)
        {
            return new BlockResponse { Success = true, Message = "Kullanıcı engelli değil." };
        }

        try
        {
            await _blockRepo.RemoveAsync(existingBlock, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new BlockResponse
            {
                Success = true,
                Message = "Engel kaldırıldı."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking user {TargetId}", request.TargetUserId);
            return new BlockResponse { Success = false, Message = "Bir hata oluştu." };
        }
    }
}
