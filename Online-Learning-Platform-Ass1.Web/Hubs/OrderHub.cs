using Microsoft.AspNetCore.SignalR;

namespace Online_Learning_Platform_Ass1.Web.Hubs;

public class OrderHub : Hub
{
    public async Task SendOrderExpired(Guid orderId)
    {
        await Clients.All.SendAsync("OrderExpired", orderId);
    }

    public async Task SendLogMessage(string message)
    {
        await Clients.All.SendAsync("LogMessage", message);
    }
}
