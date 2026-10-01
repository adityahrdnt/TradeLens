using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeLens.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "corporate_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Numerator = table.Column<int>(type: "integer", nullable: false),
                    Denominator = table.Column<int>(type: "integer", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AppliedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporate_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_corporate_actions_instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "corporate_action_applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorporateActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EligibleQuantity = table.Column<long>(type: "bigint", nullable: false),
                    ResultingQuantity = table.Column<long>(type: "bigint", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporate_action_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_corporate_action_applications_corporate_actions_CorporateAc~",
                        column: x => x.CorporateActionId,
                        principalTable: "corporate_actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_corporate_action_applications_instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_corporate_action_applications_portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_corporate_action_applications_CorporateActionId_PortfolioId",
                table: "corporate_action_applications",
                columns: new[] { "CorporateActionId", "PortfolioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_corporate_action_applications_InstrumentId",
                table: "corporate_action_applications",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_corporate_action_applications_PortfolioId_InstrumentId_Appl~",
                table: "corporate_action_applications",
                columns: new[] { "PortfolioId", "InstrumentId", "AppliedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_corporate_actions_InstrumentId_EffectiveDate_Status",
                table: "corporate_actions",
                columns: new[] { "InstrumentId", "EffectiveDate", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "corporate_action_applications");

            migrationBuilder.DropTable(
                name: "corporate_actions");
        }
    }
}
