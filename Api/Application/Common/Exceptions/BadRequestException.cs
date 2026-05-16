namespace Api.Application.Common.Exceptions;

public class BadRequestException(string message)
    : ApiException(StatusCodes.Status400BadRequest, message, "bad_request");