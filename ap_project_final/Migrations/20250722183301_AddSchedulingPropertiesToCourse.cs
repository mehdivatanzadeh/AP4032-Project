using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;


#nullable disable

namespace ap_project_final.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingPropertiesToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassDay",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClassroomId",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Courses",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ClassroomId",
                table: "Courses",
                column: "ClassroomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Classrooms_ClassroomId",
                table: "Courses",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Classrooms_ClassroomId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ClassroomId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ClassDay",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ClassroomId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Courses");
        }
    }
}
