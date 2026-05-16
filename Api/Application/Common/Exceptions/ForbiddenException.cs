namespace WebApplication1.Application.Common.Exceptions;

public class ForbiddenException(string message) : ApiException(StatusCodes.Status403Forbidden, message, "forbidden");