using Api.Application.Features.Users.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Users.SearchUsers;

public class SearchUsersHandler(IUserRepository users) : IRequestHandler<SearchUsersQuery, List<UserResponse>>
{
    public async Task<List<UserResponse>> Handle(SearchUsersQuery query, CancellationToken cancellationToken)
    {
        var search = query.Query.Trim();

        if (search.Length < 2)
            return [];

        var foundUsers = await users.SearchUsers(search, query.Limit, cancellationToken);

        return foundUsers
            .Select(user => new UserResponse(user.Id, user.Name, user.Email))
            .ToList();
    }
}
