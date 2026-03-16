using Travel.Workflows.TravelPlanCriteria.Services;

namespace Travel.Workflows.Common.Interfaces;

public interface IWorkflowFactory
{
    public Task<TravelWorkflowService> Create();
}

