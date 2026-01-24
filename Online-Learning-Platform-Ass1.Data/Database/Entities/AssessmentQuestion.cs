using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Learning_Platform_Ass1.Data.Database.Entities;

[Table("Assessment_Questions")]
public class AssessmentQuestion
{
    [Key]
    [Column("question_id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("question_text")]
    public string QuestionText { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    [Column("question_type")]
    public string QuestionType { get; set; } = "MultipleChoice"; // MultipleChoice, SkillLevel, Interest

    [Column("category_id")]
    public Guid? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [Column("order_index")]
    public int OrderIndex { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<AssessmentOption> Options { get; set; } = new List<AssessmentOption>();
}
