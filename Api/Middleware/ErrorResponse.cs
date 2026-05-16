namespace Api.Middleware;

public class ErrorResponse
{
    public int StatusCode { get; set; }

    public string ErrorCode { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Dictionary<string, string[]>? Errors { get; set; }
}