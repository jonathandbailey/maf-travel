namespace Application.Workflows.ReAct.Dto;

public record UserRequest(string Message);

public record UserResponse(string Message, Guid RequestId);

public record ActUserRequest(string Message);

public record UserInput(string Message);
