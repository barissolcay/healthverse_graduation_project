using HealthVerse.Tasks.Application.Ports;
using MediatR;

namespace HealthVerse.Tasks.Application.Commands;

public sealed record UpdateInterestsCommand(Guid UserId, List<string> ActivityTypes) : IRequest<UpdateInterestsResponse>;

public sealed class UpdateInterestsResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}

public sealed class UpdateInterestsCommandHandler : IRequestHandler<UpdateInterestsCommand, UpdateInterestsResponse>
{
    private readonly IUserInterestRepository _repository;
    private readonly ITasksUnitOfWork _unitOfWork;

    public UpdateInterestsCommandHandler(IUserInterestRepository repository, ITasksUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateInterestsResponse> Handle(UpdateInterestsCommand request, CancellationToken ct)
    {
        // Simple replace
        await _repository.ReplaceAsync(request.UserId, request.ActivityTypes, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new UpdateInterestsResponse
        {
            Success = true,
            Message = "İlgi alanları güncellendi."
        };
    }
}
