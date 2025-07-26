using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ap_project_final.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SenderStudentId = table.Column<int>(type: "int", nullable: true),
                    SenderProfessorId = table.Column<int>(type: "int", nullable: true),
                    ReceiverStudentId = table.Column<int>(type: "int", nullable: true),
                    ReceiverProfessorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Professors_ReceiverProfessorId",
                        column: x => x.ReceiverProfessorId,
                        principalTable: "Professors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Professors_SenderProfessorId",
                        column: x => x.SenderProfessorId,
                        principalTable: "Professors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Students_ReceiverStudentId",
                        column: x => x.ReceiverStudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Students_SenderStudentId",
                        column: x => x.SenderStudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverProfessorId",
                table: "Messages",
                column: "ReceiverProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverStudentId",
                table: "Messages",
                column: "ReceiverStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderProfessorId",
                table: "Messages",
                column: "SenderProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderStudentId",
                table: "Messages",
                column: "SenderStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
