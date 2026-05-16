using Api.Application.Common.Exceptions;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Users.GetMe;

public class GetMeUserHandler(IUserRepository users) : IRequestHandler<GetMeUserQuery, User>
{
    public async Task<User> Handle(GetMeUserQuery query, CancellationToken cancellationToken)
    {
        return await users.GetUser(query.UserId)
               ?? throw new NotFoundException("User not found");
    }
}
