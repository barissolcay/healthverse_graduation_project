using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using MediatR;

namespace HealthVerse.Tasks.Application.Commands;

public sealed record ClaimTaskRewardCommand(Guid TaskId, Guid UserId) : IRequest<ClaimRewardResponse>;

public sealed class ClaimTaskRewardCommandHandler : IRequestHandler<ClaimTaskRewardCommand, ClaimRewardResponse>
{
    private readonly IUserTaskRepository _userTaskRepository;
    private readonly ITaskTemplateRepository _taskTemplateRepository;
    private readonly ITasksUnitOfWork _unitOfWork;

    public ClaimTaskRewardCommandHandler(
        IUserTaskRepository userTaskRepository,
        ITaskTemplateRepository taskTemplateRepository,
        ITasksUnitOfWork unitOfWork)
    {
        _userTaskRepository = userTaskRepository;
        _taskTemplateRepository = taskTemplateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClaimRewardResponse> Handle(ClaimTaskRewardCommand request, CancellationToken ct)
    {
        var task = await _userTaskRepository.GetByIdAsync(request.TaskId, request.UserId, ct);

        if (task is null)
        {
            return new ClaimRewardResponse
            {
                Success = false,
                Message = "Görev bulunamadı."
            };
        }

        if (task.Status != UserTaskStatus.COMPLETED)
        {
            return new ClaimRewardResponse
            {
                Success = false,
                Message = task.Status == UserTaskStatus.REWARD_CLAIMED 
                    ? "Ödül zaten toplandı." 
                    : "Görev henüz tamamlanmamış."
            };
        }

        // Domain logic to claim
        task.ClaimReward();

        await _unitOfWork.SaveChangesAsync(ct);

        // Fetch template to return reward details
        var template = await _taskTemplateRepository.GetByIdAsync(task.TemplateId, ct);

        return new ClaimRewardResponse
        {
            Success = true,
            Message = "Ödül toplandı!",
            PointsEarned = template?.RewardPoints,
            BadgeEarned = template?.BadgeId,
            TitleEarned = template?.TitleId
        };
    }
}
