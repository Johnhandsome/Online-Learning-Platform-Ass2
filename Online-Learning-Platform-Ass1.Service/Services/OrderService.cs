using Microsoft.EntityFrameworkCore;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.DTOs.Order;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class OrderService(
    IOrderRepository orderRepository, 
    ICourseRepository courseRepository,
    ILearningPathRepository learningPathRepository,
    IEnrollmentRepository enrollmentRepository) : IOrderService
{
    public async Task<OrderViewModel?> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        decimal amount = 0;
        string title = "";

        if (dto.CourseId.HasValue)
        {
            // Check if already enrolled
            if (await enrollmentRepository.IsEnrolledAsync(userId, dto.CourseId.Value))
            {
                return null; // Already enrolled
            }

            var course = await courseRepository.GetByIdAsync(dto.CourseId.Value);
            if (course == null) return null;
            amount = course.Price;
            title = course.Title;
        }
        else if (dto.PathId.HasValue)
        {
            var path = await learningPathRepository.GetByIdAsync(dto.PathId.Value);
            if (path == null) return null;

            // Check if already enrolled in ALL courses of the path (meaning "Joined Path")
            // Assuming PathCourses is loaded. If not, we might need to load it. 
            // Based on existing ProcessPaymentAsync using path.PathCourses, we assume it is loaded or available.
            // Check if user is enrolled in any course of the path? Or all?
            // "Already joined path" -> Usually means enrolled in the path logic. 
            // Since we don't have PathEnrollment table, we check if user owns all courses?
            // Or typically, check if they own the first one?
            // Let's check if they own ALL.
            
            bool alreadyOwnedAll = true;
            if (path.PathCourses != null && path.PathCourses.Any())
            {
                foreach (var pc in path.PathCourses)
                {
                    if (!await enrollmentRepository.IsEnrolledAsync(userId, pc.CourseId))
                    {
                        alreadyOwnedAll = false;
                        break;
                    }
                }
            }
            else 
            {
                 alreadyOwnedAll = false; // Empty path? Validation?
            }

            if (alreadyOwnedAll && path.PathCourses != null && path.PathCourses.Any())
            {
                return null; // Already enrolled in the path
            }

            amount = path.Price;
            title = path.Title;
        }
        else
        {
            return null; // Invalid request
        }

        // Create Order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = dto.CourseId,
            PathId = dto.PathId,
            TotalAmount = amount,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1) // Order expires in 1 minute (for testing)
        };

        await orderRepository.AddAsync(order);
        await orderRepository.SaveChangesAsync();

        return new OrderViewModel
        {
            OrderId = order.Id,
            CourseId = dto.CourseId ?? Guid.Empty, // Or handle nullability in VM
            CourseTitle = title, // Reused prop for title
            Amount = order.TotalAmount,
            Status = order.Status
        };
    }

    public async Task<OrderViewModel?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        return new OrderViewModel
        {
            OrderId = order.Id,
            CourseId = order.CourseId ?? Guid.Empty,
            CourseTitle = order.Course?.Title ?? order.LearningPath?.Title ?? "Unknown",
            Amount = order.TotalAmount,
            Status = order.Status
        };
    }

    public async Task<Order?> GetOrderEntityByIdAsync(Guid orderId)
    {
        return await orderRepository.GetByIdAsync(orderId);
    }

    public async Task<bool> ProcessPaymentAsync(Guid orderId, string? transactionGateId = null)
    {
        // 1. Check for idempotency - if this transaction was already processed
        if (!string.IsNullOrEmpty(transactionGateId))
        {
            var existingTransaction = await orderRepository.GetTransactionByGatewayIdAsync(transactionGateId);
            if (existingTransaction != null)
            {
                // Transaction already processed - return success if it was successful
                return existingTransaction.Status == "success";
            }
        }

        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return false;
        }

        // 2. Check if order is already completed
        if (order.Status == "completed")
        {
            return true; // Already processed
        }

        // 3. Check if order has expired
        if (order.ExpiresAt.HasValue && order.ExpiresAt.Value < DateTime.UtcNow)
        {
            order.Status = "expired";
            await orderRepository.SaveChangesAsync();
            return false;
        }

        // 4. Use database transaction for atomicity
        using var dbTransaction = await orderRepository.BeginTransactionAsync();
        
        try
        {
            // 5. Create Transaction record
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethod = "VNPay",
                TransactionGateId = transactionGateId,
                Amount = order.TotalAmount,
                Status = "success",
                CreatedAt = DateTime.UtcNow
            };
            await orderRepository.AddTransactionAsync(transaction);

            // 6. Create Enrollment(s)
            if (order.CourseId.HasValue)
            {
                if (!await enrollmentRepository.IsEnrolledAsync(order.UserId, order.CourseId.Value))
                {
                    await CreateEnrollment(order.UserId, order.CourseId.Value);
                }
            }
            else if (order.PathId.HasValue)
            {
                var path = await learningPathRepository.GetByIdAsync(order.PathId.Value);
                if (path != null)
                {
                    foreach (var pc in path.PathCourses)
                    {
                        if (!await enrollmentRepository.IsEnrolledAsync(order.UserId, pc.CourseId))
                        {
                            await CreateEnrollment(order.UserId, pc.CourseId);
                        }
                    }
                }
            }

            // 7. Update Order Status
            order.Status = "completed";
            
            // 8. Save all changes atomically
            await orderRepository.SaveChangesAsync();
            await dbTransaction.CommitAsync();
            
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request already processed this order
            await dbTransaction.RollbackAsync();
            return false;
        }
        catch (Exception)
        {
            // Rollback on any error
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private async Task CreateEnrollment(Guid userId, Guid courseId)
    {
         var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CourseId = courseId, 
                EnrolledAt = DateTime.UtcNow,
                Status = "active"
            };
            await orderRepository.AddEnrollmentAsync(enrollment);
    }

    public async Task<IEnumerable<UserOrderDto>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await orderRepository.GetUserOrdersAsync(userId);
        var now = DateTime.UtcNow;

        return orders.Select(order =>
        {
            var itemTitle = order.CourseId.HasValue
                ? order.Course?.Title ?? "Unknown Course"
                : order.LearningPath?.Title ?? "Unknown Learning Path";

            var itemType = order.CourseId.HasValue ? "Course" : "Learning Path";

            int? minutesRemaining = null;
            bool canContinuePayment = false;

            if (order.Status == "pending" && order.ExpiresAt.HasValue)
            {
                var timeRemaining = order.ExpiresAt.Value - now;
                minutesRemaining = (int)Math.Ceiling(timeRemaining.TotalMinutes);
                
                // Only allow payment if order is still pending AND not expired
                canContinuePayment = minutesRemaining > 0;
            }

            var completedTransaction = order.Transactions?
                .FirstOrDefault(t => t.Status == "success");

            return new UserOrderDto
            {
                OrderId = order.Id,
                ItemTitle = itemTitle,
                ItemType = itemType,
                Amount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ExpiresAt = order.ExpiresAt,
                MinutesRemaining = minutesRemaining,
                CanContinuePayment = canContinuePayment,
                PaymentMethod = completedTransaction?.PaymentMethod,
                CompletedAt = completedTransaction?.CreatedAt
            };
        }).ToList();
    }
}
