using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Learning_Platform_Ass1.Data.Database.Entities;

[Table("Assessment_Options")]
public class AssessmentOption
{
    [Key]
    [Column("option_id")]
    public Guid Id { get; set; }

    [Required]
    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public AssessmentQuestion Question { get; set; } = null!;

    [Required]
    [MaxLength(300)]
    [Column("option_text")]
    public string OptionText { get; set; } = null!;

    [MaxLength(50)]
    [Column("skill_level")]
    public string? SkillLevel { get; set; } // Beginner, Intermediate, Advanced

    [Column("order_index")]
    public int OrderIndex { get; set; }
}
