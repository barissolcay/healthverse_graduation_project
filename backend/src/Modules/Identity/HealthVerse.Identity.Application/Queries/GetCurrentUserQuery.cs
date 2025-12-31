using HealthVerse.Identity.Application.DTOs;
using HealthVerse.Identity.Application.Ports;
using MediatR;

namespace HealthVerse.Identity.Application.Queries;

/// <summary>
/// Get current user information.
/// </summary>
public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserResponse?>;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CurrentUserResponse?> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            return null;
        }

        return new CurrentUserResponse
        {
            UserId = user.Id,
            Username = user.Username.Value,
            Email = user.Email.Value,
            AvatarId = user.AvatarId,
            City = user.City,
            Bio = user.Bio,
            TotalPoints = user.TotalPoints,
            StreakCount = user.StreakCount,
            FreezeInventory = user.FreezeInventory,
            CurrentTier = user.CurrentTier,
            SelectedTitleId = user.SelectedTitleId
        };
    }
}
