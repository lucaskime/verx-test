using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalanceProjection.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropAccountBalancesUpdatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountBalances_UpdatedAt",
                schema: "balance_projection",
                table: "AccountBalances");

            // Rows are rewritten on every event: leave room for HOT updates and vacuum early.
            migrationBuilder.Sql("""
                ALTER TABLE balance_projection."AccountBalances" SET (
                    fillfactor = 70,
                    autovacuum_vacuum_scale_factor = 0.01,
                    autovacuum_vacuum_threshold = 50);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE balance_projection."AccountBalances" RESET (
                    fillfactor, autovacuum_vacuum_scale_factor, autovacuum_vacuum_threshold);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_UpdatedAt",
                schema: "balance_projection",
                table: "AccountBalances",
                column: "UpdatedAt");
        }
    }
}
