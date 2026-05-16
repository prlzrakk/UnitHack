using MediatR;
using Shared.Models.Entities;

namespace Api.Application.Features.Users.GetMe;

public record GetMeUserQuery(int UserId) : IRequest<User?>;
