using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ap_project_final.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeAppealFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GradeAppeals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    StudentMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfessorResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppealDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeAppeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeAppeals_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GradeAppeals_EnrollmentId",
                table: "GradeAppeals",
                column: "EnrollmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GradeAppeals");
        }
    }
}
