using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using MediatR;

namespace HealthVerse.Tasks.Application.Commands;

public sealed record CreateGoalCommand(
    Guid UserId,
    string Title,
    string TargetMetric,
    int TargetValue,
    DateTimeOffset ValidUntil,
    string? Description = null,
    string? ActivityType = null) : IRequest<CreateGoalResponse>;

public sealed class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, CreateGoalResponse>
{
    private readonly IUserGoalRepository _repository;
    private readonly ITasksUnitOfWork _unitOfWork;

    public CreateGoalCommandHandler(IUserGoalRepository repository, ITasksUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateGoalResponse> Handle(CreateGoalCommand request, CancellationToken ct)
    {
        try
        {
            var goal = UserGoal.Create(
                request.UserId,
                request.Title,
                request.TargetMetric,
                request.TargetValue,
                request.ValidUntil,
                request.Description,
                request.ActivityType
            );

            await _repository.AddAsync(goal, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new CreateGoalResponse
            {
                Success = true,
                Message = "Hedef oluşturuldu!",
                GoalId = goal.Id
            };
        }
        catch (Exception ex)
        {
            // Domain exception handling could be done via middleware or catching DomainException specifically
            return new CreateGoalResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
