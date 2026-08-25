using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentDerogationVoid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                schema: "dbo",
                table: "ComponentDerogations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAtUtc",
                schema: "dbo",
                table: "ComponentDerogations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidedByUserId",
                schema: "dbo",
                table: "ComponentDerogations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDerogations_VoidedByUserId",
                schema: "dbo",
                table: "ComponentDerogations",
                column: "VoidedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentDerogations_AspNetUsers_VoidedByUserId",
                schema: "dbo",
                table: "ComponentDerogations",
                column: "VoidedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentDerogations_AspNetUsers_VoidedByUserId",
                schema: "dbo",
                table: "ComponentDerogations");

            migrationBuilder.DropIndex(
                name: "IX_ComponentDerogations_VoidedByUserId",
                schema: "dbo",
                table: "ComponentDerogations");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                schema: "dbo",
                table: "ComponentDerogations");

            migrationBuilder.DropColumn(
                name: "VoidedAtUtc",
                schema: "dbo",
                table: "ComponentDerogations");

            migrationBuilder.DropColumn(
                name: "VoidedByUserId",
                schema: "dbo",
                table: "ComponentDerogations");
        }
    }
}
