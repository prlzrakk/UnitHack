namespace Api.Application.Common.Exceptions;

public abstract class ApiException(
    int statusCode,
    string message,
    string errorCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
}