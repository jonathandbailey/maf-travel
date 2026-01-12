using Application.Workflows;
using Application.Workflows.Dto;

namespace Application.Services;

public interface ITravelWorkflowService
{
    Task<WorkflowResponse> PlanVacation(WorkflowRequest request);
}