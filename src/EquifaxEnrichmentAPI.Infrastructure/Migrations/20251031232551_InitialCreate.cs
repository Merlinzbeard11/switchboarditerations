using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquifaxEnrichmentAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consumer_enrichments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumer_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    match_confidence = table.Column<double>(type: "double precision", precision: 5, scale: 4, nullable: false),
                    match_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_freshness_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    personal_info_json = table.Column<string>(type: "text", nullable: false),
                    addresses_json = table.Column<string>(type: "text", nullable: false),
                    phones_json = table.Column<string>(type: "text", nullable: false),
                    financial_json = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumer_enrichments", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_consumer_enrichments_consumer_key",
                table: "consumer_enrichments",
                column: "consumer_key");

            migrationBuilder.CreateIndex(
                name: "ix_consumer_enrichments_created_at",
                table: "consumer_enrichments",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_consumer_enrichments_normalized_phone_unique",
                table: "consumer_enrichments",
                column: "normalized_phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consumer_enrichments");
        }
    }
}
