namespace WebApplication1.Application.Common.Exceptions;

public class UnauthorizedException(string message)
    : ApiException(StatusCodes.Status401Unauthorized, message, "unauthorized");