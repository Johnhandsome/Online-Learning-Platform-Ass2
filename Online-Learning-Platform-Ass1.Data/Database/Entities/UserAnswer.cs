using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Learning_Platform_Ass1.Data.Database.Entities;

[Table("User_Answers")]
public class UserAnswer
{
    [Key]
    [Column("answer_id")]
    public Guid Id { get; set; }

    [Required]
    [Column("assessment_id")]
    public Guid AssessmentId { get; set; }

    [ForeignKey(nameof(AssessmentId))]
    public UserAssessment Assessment { get; set; } = null!;

    [Required]
    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public AssessmentQuestion Question { get; set; } = null!;

    [Required]
    [Column("selected_option_id")]
    public Guid SelectedOptionId { get; set; }

    [ForeignKey(nameof(SelectedOptionId))]
    public AssessmentOption SelectedOption { get; set; } = null!;
}
