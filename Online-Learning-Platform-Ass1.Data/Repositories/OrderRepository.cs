using Microsoft.EntityFrameworkCore.Storage;
using Online_Learning_Platform_Ass1.Data.Database;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Repositories;

public class OrderRepository(OnlineLearningContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await context.Orders
            .Include(o => o.Course)
            .Include(o => o.LearningPath)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Transaction?> GetTransactionByGatewayIdAsync(string gatewayId)
    {
        return await context.Transactions
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t => t.TransactionGateId == gatewayId);
    }

    public async Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync()
    {
        return await context.Orders
            .Where(o => o.Status == "pending" && 
                        o.ExpiresAt.HasValue && 
                        o.ExpiresAt.Value < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
    {
        return await context.Orders
            .Include(o => o.Course)
            .Include(o => o.LearningPath)
            .Include(o => o.Transactions)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await context.Orders.AddAsync(order);
    }

    public async Task AddEnrollmentAsync(Enrollment enrollment)
    {
        await context.Enrollments.AddAsync(enrollment);
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        await context.Transactions.AddAsync(transaction);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await context.Database.BeginTransactionAsync();
    }

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}
