using System.ComponentModel.DataAnnotations.Schema;
namespace Online_Learning_Platform_Ass1.Data.Database.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreateAt { get; set; }

    [Column("has_completed_assessment")]
    public bool HasCompletedAssessment { get; set; } = false;

    [Column("assessment_completed_at")]
    public DateTime? AssessmentCompletedAt { get; set; }

    public Guid? RoleId { get; set; }
    public Role? Role { get; set; }

    public Profile? Profile { get; set; }
}
