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

            // Get all courses in the path
            var pathCourses = path.PathCourses?.Select(pc => pc.Course).ToList() ?? new List<Data.Database.Entities.Course>();
            
            if (!pathCourses.Any())
            {
                return null; // Path has no courses
            }

            // Check which courses user already owns and calculate discount
            decimal ownedCoursesPrice = 0;
            bool alreadyOwnedAll = true;
            
            foreach (var course in pathCourses)
            {
                if (await enrollmentRepository.IsEnrolledAsync(userId, course.Id))
                {
                    ownedCoursesPrice += course.Price;
                }
                else
                {
                    alreadyOwnedAll = false;
                }
            }

            // Block purchase if user owns all courses
            if (alreadyOwnedAll)
            {
                return null; // User already owns all courses in this path
            }

            // Calculate adjusted price: Path price - Owned courses price
            amount = path.Price - ownedCoursesPrice;

            // If adjusted price is <= 0, block purchase
            if (amount <= 0)
            {
                return null; // Discount exceeds path price
            }

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
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
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

    public async Task<OrderPreviewDto?> GetOrderPreviewAsync(Guid userId, CreateOrderDto dto)
    {
        if (!dto.CourseId.HasValue && !dto.PathId.HasValue)
        {
            return null;
        }

        if (dto.CourseId.HasValue)
        {
            // Single course preview
            var course = await courseRepository.GetByIdAsync(dto.CourseId.Value);
            if (course == null) return null;

            // Check if already enrolled
            if (await enrollmentRepository.IsEnrolledAsync(userId, dto.CourseId.Value))
            {
                return new OrderPreviewDto
                {
                    CourseId = dto.CourseId,
                    ItemTitle = course.Title,
                    ItemType = "Course",
                    OriginalPrice = course.Price,
                    FinalPrice = course.Price,
                    CanPurchase = false,
                    BlockReason = "You already own this course"
                };
            }

            return new OrderPreviewDto
            {
                CourseId = dto.CourseId,
                ItemTitle = course.Title,
                ItemType = "Course",
                OriginalPrice = course.Price,
                FinalPrice = course.Price,
                CanPurchase = true
            };
        }
        else
        {
            // Learning Path preview
            var path = await learningPathRepository.GetByIdAsync(dto.PathId!.Value);
            if (path == null) return null;

            var pathCourses = path.PathCourses?.Select(pc => pc.Course).ToList() ?? new List<Data.Database.Entities.Course>();
            
            if (!pathCourses.Any())
            {
                return new OrderPreviewDto
                {
                    PathId = dto.PathId,
                    ItemTitle = path.Title,
                    ItemType = "Learning Path",
                    OriginalPrice = path.Price,
                    FinalPrice = path.Price,
                    CanPurchase = false,
                    BlockReason = "This learning path has no courses"
                };
            }

            // Check owned courses and calculate discount
            var ownedCourses = new List<OwnedCourseDto>();
            decimal totalDiscount = 0;
            bool alreadyOwnedAll = true;

            foreach (var course in pathCourses)
            {
                if (await enrollmentRepository.IsEnrolledAsync(userId, course.Id))
                {
                    ownedCourses.Add(new OwnedCourseDto
                    {
                        CourseId = course.Id,
                        Title = course.Title,
                        Price = course.Price
                    });
                    totalDiscount += course.Price;
                }
                else
                {
                    alreadyOwnedAll = false;
                }
            }

            var finalPrice = path.Price - totalDiscount;

            // Determine if can purchase
            bool canPurchase = true;
            string? blockReason = null;

            if (alreadyOwnedAll)
            {
                canPurchase = false;
                blockReason = "You already own all courses in this learning path";
            }
            else if (finalPrice <= 0)
            {
                canPurchase = false;
                blockReason = "The discount exceeds the path price. Please contact support.";
            }

            return new OrderPreviewDto
            {
                PathId = dto.PathId,
                ItemTitle = path.Title,
                ItemType = "Learning Path",
                OriginalPrice = path.Price,
                OwnedCourses = ownedCourses,
                TotalDiscount = totalDiscount,
                FinalPrice = finalPrice,
                CanPurchase = canPurchase,
                BlockReason = blockReason
            };
        }
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
            
            // Calculate display status - show expired immediately even if DB not updated yet
            var displayStatus = order.Status;
            if (order.Status == "pending" && order.ExpiresAt.HasValue && order.ExpiresAt.Value < now)
            {
                displayStatus = "expired";
            }

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
                Status = displayStatus, // Use calculated status instead of DB status
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
