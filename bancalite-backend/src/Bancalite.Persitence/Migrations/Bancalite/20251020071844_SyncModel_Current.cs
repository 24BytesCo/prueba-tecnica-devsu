using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bancalite.Persitence.Migrations.Bancalite
{
    /// <inheritdoc />
    public partial class SyncModel_Current : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clientes_AppUserId",
                table: "clientes");

            migrationBuilder.CreateIndex(
                name: "IX_personas_Email",
                table: "personas",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_AppUserId",
                table: "clientes",
                column: "AppUserId",
                unique: true,
                filter: "\"AppUserId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_personas_Email",
                table: "personas");

            migrationBuilder.DropIndex(
                name: "IX_clientes_AppUserId",
                table: "clientes");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_AppUserId",
                table: "clientes",
                column: "AppUserId");
        }
    }
}
