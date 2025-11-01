using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquifaxEnrichmentAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "buyers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_key_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buyers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_buyers_api_key_hash_unique",
                table: "buyers",
                column: "api_key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_buyers_created_at",
                table: "buyers",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_buyers_is_active",
                table: "buyers",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "buyers");
        }
    }
}
