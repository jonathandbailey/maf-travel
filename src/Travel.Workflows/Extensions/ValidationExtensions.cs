using Microsoft.Extensions.AI;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Exceptions;

namespace Travel.Workflows.Extensions;

public static class ValidationExtensions
{
    public static void Validate(this TravelWorkflowRequest request)
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
}