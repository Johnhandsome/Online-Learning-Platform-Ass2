using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Learning_Platform_Ass1.Data.Database.Entities;

[Table("Orders")]
public class Order
{
    [Key]
    [Column("order_id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    // ERD has course_id FK. This implies buying SINGLE course per order? 
    // Or is it cart based? The ERD shows order linked to course.
    [Column("course_id")]
    public Guid? CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    // ERD also shows path_id FK? "path_id" linked to Orders in "Orders" table diagram?
    // Let's support both.
    [Column("path_id")] 
    public Guid? PathId { get; set; } 
    // Will uncomment relation once LearningPath is created
    [ForeignKey(nameof(PathId))]
    public LearningPath? LearningPath { get; set; }

    [Column("total_amount", TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "pending"; // pending, completed, failed

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
