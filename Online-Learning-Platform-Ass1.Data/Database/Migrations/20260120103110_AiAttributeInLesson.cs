using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Learning_Platform_Ass1.Data.Migrations;

/// <inheritdoc />
public partial class AiAttributeInLesson : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ai_summary",
            table: "Lesson_Progress",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ai_summary_status",
            table: "Lesson_Progress",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "transcript",
            table: "Lesson_Progress",
            type: "nvarchar(max)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ai_summary",
            table: "Lesson_Progress");

        migrationBuilder.DropColumn(
            name: "ai_summary_status",
            table: "Lesson_Progress");

        migrationBuilder.DropColumn(
            name: "transcript",
            table: "Lesson_Progress");
    }
}
