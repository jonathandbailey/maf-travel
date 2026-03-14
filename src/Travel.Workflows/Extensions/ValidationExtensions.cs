using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Exceptions;

namespace Travel.Workflows.Extensions;

public static class ValidationExtensions
{
    public static void Validate(this WorkflowRunRequest request)
    {
        if (request.ThreadId == Guid.Empty)
        {
            throw new WorkflowValidationException("ThreadId cannot be Guid.Empty.", NodeNames.StartNodeName, Guid.Empty);
        }

        if (request.Message.Role != ChatRole.User)
        {
            throw new WorkflowValidationException("Message must have Role 'User'.", NodeNames.StartNodeName, request.ThreadId);
        }

        if (string.IsNullOrEmpty(request.Message.Text))
        {
            throw new WorkflowValidationException("Message Text must have Content.", NodeNames.StartNodeName, request.ThreadId);
        }
    }

    public static void Validate(this TravelPlanState travelPlan, string nodeName, Guid threadId)
    {
        if (string.IsNullOrWhiteSpace(travelPlan.Origin))
            throw new WorkflowValidationException("TravelPlan Origin is required.", nodeName, threadId);

        if (string.IsNullOrWhiteSpace(travelPlan.Destination))
            throw new WorkflowValidationException("TravelPlan Destination is required.", nodeName, threadId);

        if (travelPlan.StartDate is null)
            throw new WorkflowValidationException("TravelPlan StartDate is required.", nodeName, threadId);

        if (travelPlan.EndDate is null)
            throw new WorkflowValidationException("TravelPlan EndDate is required.", nodeName, threadId);

        if (travelPlan.NumberOfTravelers is null)
            throw new WorkflowValidationException("TravelPlan NumberOfTravelers is required.", nodeName, threadId);
    }
}