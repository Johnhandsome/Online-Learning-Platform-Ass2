using Microsoft.EntityFrameworkCore.Storage;
using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Transaction?> GetTransactionByGatewayIdAsync(string gatewayId);
    Task AddAsync(Order order);
    Task AddEnrollmentAsync(Enrollment enrollment);
    Task AddTransactionAsync(Transaction transaction);
    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
