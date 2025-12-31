using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using MediatR;

namespace HealthVerse.Social.Application.Queries;

// --- Queries ---

public sealed record GetFollowersQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<FollowListResponse>;
public sealed record GetFollowingQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<FollowListResponse>;
public sealed record GetMutualFriendsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<FollowListResponse>;

// --- Handlers ---

public sealed class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, FollowListResponse>
{
    private readonly IFriendshipRepository _friendshipRepo;

    public GetFollowersQueryHandler(IFriendshipRepository friendshipRepo)
    {
        _friendshipRepo = friendshipRepo;
    }

    public async Task<FollowListResponse> Handle(GetFollowersQuery request, CancellationToken ct)
    {
        var result = await _friendshipRepo.GetFollowersAsync(request.UserId, request.Page, request.PageSize, ct);
        
        return new FollowListResponse
        {
            Users = result.Items.Select(u => new UserSummaryDto
            {
                Id = u.UserId,
                Username = u.Username,
                AvatarId = u.AvatarId,
                TotalPoints = u.TotalPoints,
                CurrentTier = u.CurrentTier,
                SelectedTitleId = u.SelectedTitleId
            }).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}

public sealed class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, FollowListResponse>
{
    private readonly IFriendshipRepository _friendshipRepo;

    public GetFollowingQueryHandler(IFriendshipRepository friendshipRepo)
    {
        _friendshipRepo = friendshipRepo;
    }

    public async Task<FollowListResponse> Handle(GetFollowingQuery request, CancellationToken ct)
    {
        var result = await _friendshipRepo.GetFollowingAsync(request.UserId, request.Page, request.PageSize, ct);

        return new FollowListResponse
        {
            Users = result.Items.Select(u => new UserSummaryDto
            {
                Id = u.UserId,
                Username = u.Username,
                AvatarId = u.AvatarId,
                TotalPoints = u.TotalPoints,
                CurrentTier = u.CurrentTier,
                SelectedTitleId = u.SelectedTitleId
            }).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}

public sealed class GetMutualFriendsQueryHandler : IRequestHandler<GetMutualFriendsQuery, FollowListResponse>
{
    private readonly IFriendshipRepository _friendshipRepo;

    public GetMutualFriendsQueryHandler(IFriendshipRepository friendshipRepo)
    {
        _friendshipRepo = friendshipRepo;
    }

    public async Task<FollowListResponse> Handle(GetMutualFriendsQuery request, CancellationToken ct)
    {
        var result = await _friendshipRepo.GetMutualFriendsAsync(request.UserId, request.Page, request.PageSize, ct);

        return new FollowListResponse
        {
            Users = result.Items.Select(u => new UserSummaryDto
            {
                Id = u.UserId,
                Username = u.Username,
                AvatarId = u.AvatarId,
                TotalPoints = u.TotalPoints,
                CurrentTier = u.CurrentTier,
                SelectedTitleId = u.SelectedTitleId
            }).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}
