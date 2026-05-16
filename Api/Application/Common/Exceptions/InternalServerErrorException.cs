namespace WebApplication1.Application.Common.Exceptions;

public class InternalServerErrorException(string message) 
    : ApiException(StatusCodes.Status500InternalServerError, message, "internal_server_error");