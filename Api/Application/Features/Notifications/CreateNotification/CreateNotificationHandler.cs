using Api.Application.Common.Exceptions;
using Api.Application.Features.Notifications.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Notifications.CreateNotification;

public class CreateNotificationHandler(
    IUserRepository users,
    IKanbanRepository kanbans,
    IKanbanTaskRepository tasks,
    INotificationRepository notifications,
    IUnitOfWork unitOfWork,
    INotificationSender sender) : IRequestHandler<CreateNotificationCommand, NotificationResponse>
{
    public async Task<NotificationResponse> Handle(
        CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        _ = await users.GetUser(command.UserId)
            ?? throw new NotFoundException("User not found");
        var task = await tasks.GetByIdAsync(command.TaskId, cancellationToken)
                   ?? throw new NotFoundException("Task not found");
        var kanban = await kanbans.GetByIdAsync(command.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        if (task.KanbanId != kanban.Id)
            throw new BadRequestException("Task does not belong to kanban");

        var notification = await notifications.AddAsync(
            command.UserId,
            command.TaskId,
            command.KanbanId,
            command.Name,
            command.Message,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var response = notification.ToResponse();
        await sender.SendToUserAsync(command.UserId, response, cancellationToken);
        return response;
    }
}
