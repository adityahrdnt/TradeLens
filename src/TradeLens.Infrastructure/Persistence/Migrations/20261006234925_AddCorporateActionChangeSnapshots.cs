using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeLens.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateActionChangeSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewDenominator",
                table: "corporate_action_changes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NewExDate",
                table: "corporate_action_changes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NewNumerator",
                table: "corporate_action_changes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NewRecordDate",
                table: "corporate_action_changes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousDenominator",
                table: "corporate_action_changes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PreviousExDate",
                table: "corporate_action_changes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousNumerator",
                table: "corporate_action_changes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PreviousRecordDate",
                table: "corporate_action_changes",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewDenominator",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "NewExDate",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "NewNumerator",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "NewRecordDate",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "PreviousDenominator",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "PreviousExDate",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "PreviousNumerator",
                table: "corporate_action_changes");

            migrationBuilder.DropColumn(
                name: "PreviousRecordDate",
                table: "corporate_action_changes");
        }
    }
}
