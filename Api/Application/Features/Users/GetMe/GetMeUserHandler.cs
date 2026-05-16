using Client.Models.Entities;
using Infrastructure.Interfaces;
using MediatR;

namespace Api.Application.Features.Users.GetMe;

public class GetMeUserHandler(IUserRepository users) : IRequestHandler<GetMeUserQuery, User?>
{
    public Task<User?> Handle(GetMeUserQuery query, CancellationToken cancellationToken)
    {
        return users.GetUser(query.UserId);
    }
}
