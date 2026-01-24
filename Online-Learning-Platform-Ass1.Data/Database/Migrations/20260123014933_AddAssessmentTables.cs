using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Learning_Platform_Ass1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assessment_Questions",
                columns: table => new
                {
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    question_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment_Questions", x => x.question_id);
                    table.ForeignKey(
                        name: "FK_Assessment_Questions_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "User_Assessments",
                columns: table => new
                {
                    assessment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Assessments", x => x.assessment_id);
                    table.ForeignKey(
                        name: "FK_User_Assessments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessment_Options",
                columns: table => new
                {
                    option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    option_text = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    skill_level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment_Options", x => x.option_id);
                    table.ForeignKey(
                        name: "FK_Assessment_Options_Assessment_Questions_question_id",
                        column: x => x.question_id,
                        principalTable: "Assessment_Questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Answers",
                columns: table => new
                {
                    answer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    selected_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Answers", x => x.answer_id);
                    table.ForeignKey(
                        name: "FK_User_Answers_Assessment_Options_selected_option_id",
                        column: x => x.selected_option_id,
                        principalTable: "Assessment_Options",
                        principalColumn: "option_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Answers_Assessment_Questions_question_id",
                        column: x => x.question_id,
                        principalTable: "Assessment_Questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Answers_User_Assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "User_Assessments",
                        principalColumn: "assessment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_Options_question_id",
                table: "Assessment_Options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_Questions_category_id",
                table: "Assessment_Questions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Answers_assessment_id",
                table: "User_Answers",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Answers_question_id",
                table: "User_Answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Answers_selected_option_id",
                table: "User_Answers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Assessments_user_id",
                table: "User_Assessments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User_Answers");

            migrationBuilder.DropTable(
                name: "Assessment_Options");

            migrationBuilder.DropTable(
                name: "User_Assessments");

            migrationBuilder.DropTable(
                name: "Assessment_Questions");
        }
    }
}
