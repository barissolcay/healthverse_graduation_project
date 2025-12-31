using HealthVerse.Tasks.Application.Ports;
using MediatR;

namespace HealthVerse.Tasks.Application.Commands;

public sealed record DeleteGoalCommand(Guid GoalId, Guid UserId) : IRequest<DeleteGoalResponse>;

public sealed class DeleteGoalResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}

public sealed class DeleteGoalCommandHandler : IRequestHandler<DeleteGoalCommand, DeleteGoalResponse>
{
    private readonly IUserGoalRepository _repository;
    private readonly ITasksUnitOfWork _unitOfWork;

    public DeleteGoalCommandHandler(IUserGoalRepository repository, ITasksUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteGoalResponse> Handle(DeleteGoalCommand request, CancellationToken ct)
    {
        var goal = await _repository.GetByIdAsync(request.GoalId, request.UserId, ct);

        if (goal is null)
        {
            return new DeleteGoalResponse
            {
                Success = false,
                Message = "Hedef bulunamadı."
            };
        }

        if (goal.IsCompleted)
        {
            return new DeleteGoalResponse
            {
                Success = false,
                Message = "Tamamlanan hedefler silinemez."
            };
        }

        await _repository.RemoveAsync(goal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new DeleteGoalResponse
        {
            Success = true,
            Message = "Hedef silindi."
        };
    }
}
