using HealthVerse.Tasks.Application.DTOs;
using HealthVerse.Tasks.Application.Ports;
using MediatR;

namespace HealthVerse.Tasks.Application.Queries;

public sealed record GetTaskTemplatesQuery(bool ActiveOnly = true) : IRequest<List<TaskTemplateDto>>;

public sealed class GetTaskTemplatesQueryHandler : IRequestHandler<GetTaskTemplatesQuery, List<TaskTemplateDto>>
{
    private readonly ITaskTemplateRepository _repository;

    public GetTaskTemplatesQueryHandler(ITaskTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TaskTemplateDto>> Handle(GetTaskTemplatesQuery request, CancellationToken ct)
    {
        var templates = await _repository.GetAllAsync(request.ActiveOnly, ct);

        return templates.Select(t => new TaskTemplateDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Category = t.Category,
            ActivityType = t.ActivityType,
            TargetMetric = t.TargetMetric,
            TargetValue = t.TargetValue,
            RewardPoints = t.RewardPoints,
            BadgeId = t.BadgeId,
            TitleId = t.TitleId,
            IsActive = t.IsActive
        }).ToList();
    }
}
