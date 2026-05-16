namespace Api.Application.Common.Exceptions;

public class ApiException(int code, string message) : Exception(message)
{
    public int Code { get; } = code;
}