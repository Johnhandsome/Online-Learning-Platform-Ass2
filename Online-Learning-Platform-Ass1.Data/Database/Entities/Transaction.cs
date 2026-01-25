using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Learning_Platform_Ass1.Data.Database.Entities;

[Table("Transactions")]
public class Transaction
{
    [Key]
    [Column("transaction_id")]
    public Guid Id { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    [Column("payment_method")]
    public string PaymentMethod { get; set; } = null!; // paypal, credit_card

    [Column("transaction_gate_id")]
    [MaxLength(100)]
    public string? TransactionGateId { get; set; } // ID from payment gateway (unique from VNPay)

    [Column("amount", TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "pending"; // success, failed

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
