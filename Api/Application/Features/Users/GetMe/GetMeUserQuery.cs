using Client.Models.Entities;
using MediatR;

namespace Api.Application.Features.Users.GetMe;

public record GetMeUserQuery(int UserId) : IRequest<User?>;
