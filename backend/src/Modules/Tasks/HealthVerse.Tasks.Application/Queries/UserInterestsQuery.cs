using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using MediatR;

namespace HealthVerse.Tasks.Application.Queries;

// --- Queries ---

public sealed record GetUserInterestsQuery(Guid UserId) : IRequest<InterestsResponse>;

// --- DTOs ---

public sealed class InterestsResponse
{
    public List<string> ActivityTypes { get; init; } = new();
    public int TotalInterests { get; init; }
}

// --- Handlers ---

public sealed class UserInterestsQueryHandlers : IRequestHandler<GetUserInterestsQuery, InterestsResponse>
{
    private readonly IUserInterestRepository _repository;

    public UserInterestsQueryHandlers(IUserInterestRepository repository)
    {
        _repository = repository;
    }

    public async Task<InterestsResponse> Handle(GetUserInterestsQuery request, CancellationToken ct)
    {
        var interests = await _repository.GetActivityTypesAsync(request.UserId, ct);
        
        return new InterestsResponse
        {
            ActivityTypes = interests,
            TotalInterests = interests.Count
        };
    }
}
