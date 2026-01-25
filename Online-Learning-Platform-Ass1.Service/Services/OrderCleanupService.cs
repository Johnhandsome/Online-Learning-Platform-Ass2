using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.Hubs;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class OrderCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<OrderHub> _hubContext;

    public OrderCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderCleanupService> logger,
        IConfiguration configuration,
        IHubContext<OrderHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Order Cleanup Service started");

        // Get interval from config (default 1 minute for testing)
        var intervalMinutes = _configuration.GetValue<int>("OrderCleanup:IntervalMinutes", 1);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredOrders(stoppingToken);
                
                // Wait before next cleanup
                _logger.LogInformation("⏰ Next cleanup in {Minutes} minute(s)", intervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // App is shutting down
                _logger.LogInformation("Order Cleanup Service is stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in Order Cleanup Service");
                // Wait 1 minute before retry on error
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("🛑 Order Cleanup Service stopped");
    }

    private async Task CleanupExpiredOrders(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        _logger.LogInformation("🔍 Checking for expired orders...");

        var expiredOrders = await orderRepository.GetExpiredPendingOrdersAsync();

        if (!expiredOrders.Any())
        {
            _logger.LogInformation("✅ No expired orders found");
            return;
        }

        var expiredOrderIds = new List<Guid>();

        foreach (var order in expiredOrders)
        {
            order.Status = "expired";
            expiredOrderIds.Add(order.Id);
            _logger.LogWarning(
                "⏱️ Expired order {OrderId} for user {UserId} (created: {CreatedAt}, expired: {ExpiresAt})",
                order.Id,
                order.UserId,
                order.CreatedAt,
                order.ExpiresAt);
        }

        await orderRepository.SaveChangesAsync();
        
        _logger.LogInformation(
            "🧹 Cleaned up {Count} expired order(s)", 
            expiredOrders.Count());

        // Push SignalR notification to all connected clients
        await _hubContext.Clients.All.SendAsync("OrdersExpired", expiredOrderIds, cancellationToken);
        _logger.LogInformation("📡 Sent SignalR notification for {Count} expired orders", expiredOrderIds.Count);
    }
}
