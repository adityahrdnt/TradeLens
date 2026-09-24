using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeLens.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "market_prices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    PriceTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_market_prices_instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_market_prices_InstrumentId_PriceTimestamp",
                table: "market_prices",
                columns: new[] { "InstrumentId", "PriceTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_market_prices_InstrumentId_Source_PriceTimestamp",
                table: "market_prices",
                columns: new[] { "InstrumentId", "Source", "PriceTimestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "market_prices");
        }
    }
}
