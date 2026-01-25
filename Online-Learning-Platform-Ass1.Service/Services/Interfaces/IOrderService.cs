using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Service.DTOs.Order;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;

public interface IOrderService
{
    Task<OrderViewModel?> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<OrderViewModel?> GetOrderByIdAsync(Guid orderId);
    Task<Order?> GetOrderEntityByIdAsync(Guid orderId);
    Task<bool> ProcessPaymentAsync(Guid orderId, string? transactionGateId = null);
    Task<IEnumerable<UserOrderDto>> GetUserOrdersAsync(Guid userId);
}
