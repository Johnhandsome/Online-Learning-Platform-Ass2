using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

public class ChatHub : Hub<IChatClient>
{
    private static readonly ConcurrentDictionary<string, string> AdminConnections = new();
    private static readonly ConcurrentDictionary<string, string> UserConnections = new();
    private static readonly ConcurrentDictionary<string, string> ActiveChats = new(); // UserId -> AdminId

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (role == "Admin")
        {
            AdminConnections[Context.ConnectionId] = userId ?? "Admin";
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }
        else
        {
            UserConnections[Context.ConnectionId] = userId ?? "Guest";
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        AdminConnections.TryRemove(Context.ConnectionId, out _);
        UserConnections.TryRemove(Context.ConnectionId, out _);
        ActiveChats.TryRemove(Context.ConnectionId, out _);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RequestLiveSupport()
    {
        var userName = Context.User?.Identity?.Name ?? "Guest";
        await Clients.Group("Admins").UserRequestedSupport(Context.ConnectionId, userName);
    }

    public async Task SendMessageToAdmin(string message)
    {
        var userName = Context.User?.Identity?.Name ?? "Guest";
        
        if (ActiveChats.TryGetValue(Context.ConnectionId, out var adminConnectionId))
        {
            await Clients.Client(adminConnectionId).ReceiveMessage(userName, message, DateTime.UtcNow);
        }
        else
        {
            await Clients.Group("Admins").ReceiveMessage(userName, message, DateTime.UtcNow);
        }
    }

    public async Task JoinUserChat(string userConnectionId)
    {
        var adminName = Context.User?.Identity?.Name ?? "Admin";
        ActiveChats[userConnectionId] = Context.ConnectionId;
        
        await Clients.Client(userConnectionId).AdminJoined(adminName);
    }

    public async Task SendMessageToUser(string userConnectionId, string message)
    {
        var adminName = Context.User?.Identity?.Name ?? "Admin";
        await Clients.Client(userConnectionId).ReceiveMessage(adminName, message, DateTime.UtcNow);
    }

    public async Task EndChat(string userConnectionId)
    {
        ActiveChats.TryRemove(userConnectionId, out _);
        await Clients.Client(userConnectionId).SupportEnded();
    }
}
