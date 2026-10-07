using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeveloperProjectTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_EntityType_EntityId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ActivityType",
                table: "ActivityLogs");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "ActivityLogs",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "ActivityLogs",
                newName: "TaskId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "ActivityLogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ActivityLogs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ActivityLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TaskId",
                table: "ActivityLogs",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TenantId",
                table: "ActivityLogs",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Tasks_TaskId",
                table: "ActivityLogs",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Tenants_TenantId",
                table: "ActivityLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Tasks_TaskId",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Tenants_TenantId",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_TaskId",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_TenantId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ActivityLogs");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "ActivityLogs",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "ActivityLogs",
                newName: "EntityType");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "ActivityLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ActivityLogs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "ActivityType",
                table: "ActivityLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_EntityType_EntityId",
                table: "ActivityLogs",
                columns: new[] { "EntityType", "EntityId" });
        }
    }
}
