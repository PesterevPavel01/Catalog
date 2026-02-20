using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserToInitiator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_approval_workflow_items_application_users_UserId",
                table: "approval_workflow_items");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "approval_workflow_items",
                newName: "InitiatorId");

            migrationBuilder.RenameIndex(
                name: "IX_approval_workflow_items_UserId",
                table: "approval_workflow_items",
                newName: "IX_approval_workflow_items_InitiatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_approval_workflow_items_application_users_InitiatorId",
                table: "approval_workflow_items",
                column: "InitiatorId",
                principalTable: "application_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_approval_workflow_items_application_users_InitiatorId",
                table: "approval_workflow_items");

            migrationBuilder.RenameColumn(
                name: "InitiatorId",
                table: "approval_workflow_items",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_approval_workflow_items_InitiatorId",
                table: "approval_workflow_items",
                newName: "IX_approval_workflow_items_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_approval_workflow_items_application_users_UserId",
                table: "approval_workflow_items",
                column: "UserId",
                principalTable: "application_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
