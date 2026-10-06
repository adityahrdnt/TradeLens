using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeLens.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateActionChangeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "OriginalEffectiveDate",
                table: "corporate_actions",
                type: "date",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE corporate_actions
                SET "OriginalEffectiveDate" = "EffectiveDate";
                """);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OriginalEffectiveDate",
                table: "corporate_actions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "corporate_action_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorporateActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    PreviousEffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewEffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporate_action_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_corporate_action_changes_corporate_actions_CorporateActionId",
                        column: x => x.CorporateActionId,
                        principalTable: "corporate_actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_corporate_action_changes_CorporateActionId_ChangedAt",
                table: "corporate_action_changes",
                columns: new[] { "CorporateActionId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "OriginalEffectiveDate",
                table: "corporate_actions");
        }
    }
}
