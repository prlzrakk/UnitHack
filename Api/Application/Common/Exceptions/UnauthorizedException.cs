namespace Api.Application.Common.Exceptions;

public class UnauthorizedException(string message)
    : ApiException(StatusCodes.Status401Unauthorized, message, "unauthorized");