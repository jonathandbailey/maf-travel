using Travel.Workflows.Services;

namespace Travel.Workflows.Interfaces;

public interface IWorkflowFactory
{
    public Task<TravelWorkflowService> Create();
}

