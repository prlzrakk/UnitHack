namespace Api.Application.Common.Exceptions;

public class NotFoundException(string message)
    : ApiException(StatusCodes.Status404NotFound, message, "not_found");