using Travel.Workflows;
using Travel.Workflows.Dto;

namespace Application.Services;

public interface ITravelWorkflowService
{
    Task<WorkflowResponse> PlanVacation(WorkflowRequest request);
}