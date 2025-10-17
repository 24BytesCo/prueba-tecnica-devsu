using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bancalite.Persitence.Migrations.Bancalite
{
    /// <inheritdoc />
    public partial class AddMovimientoIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "movimientos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_CuentaId_IdempotencyKey",
                table: "movimientos",
                columns: new[] { "CuentaId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_movimientos_CuentaId_IdempotencyKey",
                table: "movimientos");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "movimientos");
        }
    }
}
