using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeLens.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateActionEligibilityDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EligibilityDate",
                table: "corporate_actions",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "NewEligibilityDate",
                table: "corporate_action_changes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PreviousEligibilityDate",
                table: "corporate_action_changes",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EligibilityDate",
                table: "corporate_actions");

            migrationBuilder.DropColumn(
                name: "NewEligibilityDate",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "PreviousEligibilityDate",
                table: "corporate_action_changes");
        }
    }
}
