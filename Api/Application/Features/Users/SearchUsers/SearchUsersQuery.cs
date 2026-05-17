using Api.Application.Features.Users.Common;
using MediatR;

namespace Api.Application.Features.Users.SearchUsers;

public record SearchUsersQuery(string Query, int Limit) : IRequest<List<UserResponse>>;
