using Microsoft.AspNetCore.SignalR;

namespace Api.Application.Hubs;

public sealed class NotificationUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("user_id")?.Value;
    }
}
