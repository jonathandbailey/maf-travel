namespace ConsoleApp.Dto
{
    public class UserRequestDto
    {
        public Guid SessionId { get; init; } = Guid.Empty;

        public string Message { get; init; } = string.Empty;

        public string TaskId { get; init; } = string.Empty;
    }
}
