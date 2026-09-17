using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeLens.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyResponseSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PositionAveragePrice",
                table: "idempotency_records",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PositionCostBasis",
                table: "idempotency_records",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "idempotency_records",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "PositionQuantity",
                table: "idempotency_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionAveragePrice",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "PositionCostBasis",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "PositionQuantity",
                table: "idempotency_records");
        }
    }
}
