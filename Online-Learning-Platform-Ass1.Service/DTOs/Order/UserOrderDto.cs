namespace Online_Learning_Platform_Ass1.Service.DTOs.Order;

public record UserOrderDto
{
    public Guid OrderId { get; init; }
    public string ItemTitle { get; init; } = null!;
    public string ItemType { get; init; } = null!; // "Course" or "Learning Path"
    public decimal Amount { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public int? MinutesRemaining { get; init; }
    public bool CanContinuePayment { get; init; }
    public string? PaymentMethod { get; init; }
    public DateTime? CompletedAt { get; init; }
}
