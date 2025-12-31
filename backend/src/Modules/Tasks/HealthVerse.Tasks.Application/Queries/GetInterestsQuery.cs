using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using MediatR;

namespace HealthVerse.Tasks.Application.Queries;

public sealed record GetInterestsQuery(Guid UserId) : IRequest<InterestsResponse>;

public sealed class GetInterestsQueryHandler : IRequestHandler<GetInterestsQuery, InterestsResponse>
{
    private readonly IUserInterestRepository _repository;

    public GetInterestsQueryHandler(IUserInterestRepository repository)
    {
        _repository = repository;
    }

    public async Task<InterestsResponse> Handle(GetInterestsQuery request, CancellationToken ct)
    {
        var interests = await _repository.GetActivityTypesAsync(request.UserId, ct);

        return new InterestsResponse
        {
            ActivityTypes = interests,
            TotalInterests = interests.Count
        };
    }
}
