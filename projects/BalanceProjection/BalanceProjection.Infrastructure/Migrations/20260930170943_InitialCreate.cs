using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalanceProjection.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "balance_projection");

            migrationBuilder.CreateTable(
                name: "AccountBalances",
                schema: "balance_projection",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredits = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDebits = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionCount = table.Column<long>(type: "bigint", nullable: false),
                    LastTransactionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalances", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "BusinessAcks",
                schema: "balance_projection",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAcks", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedTransactions",
                schema: "balance_projection",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedTransactions", x => x.TransactionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_UpdatedAt",
                schema: "balance_projection",
                table: "AccountBalances",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAcks_ProcessedAt",
                schema: "balance_projection",
                table: "BusinessAcks",
                column: "ProcessedAt",
                filter: "\"PublishedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedTransactions_ProcessedAt",
                schema: "balance_projection",
                table: "ProcessedTransactions",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedTransactions_RawTransactionId",
                schema: "balance_projection",
                table: "ProcessedTransactions",
                column: "RawTransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBalances",
                schema: "balance_projection");

            migrationBuilder.DropTable(
                name: "BusinessAcks",
                schema: "balance_projection");

            migrationBuilder.DropTable(
                name: "ProcessedTransactions",
                schema: "balance_projection");
        }
    }
}
