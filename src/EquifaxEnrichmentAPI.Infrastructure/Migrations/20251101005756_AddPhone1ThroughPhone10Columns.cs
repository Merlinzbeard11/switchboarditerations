using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquifaxEnrichmentAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhone1ThroughPhone10Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            // Create individual B-tree indexes on each phone column
            // PostgreSQL will combine these using bitmap index scan for multi-column OR queries
            // Reference: PostgreSQL documentation on bitmap index scans for OR queries
            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone1",
                table: "consumer_enrichments",
                column: "Phone1");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone2",
                table: "consumer_enrichments",
                column: "Phone2");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone3",
                table: "consumer_enrichments",
                column: "Phone3");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone4",
                table: "consumer_enrichments",
                column: "Phone4");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone5",
                table: "consumer_enrichments",
                column: "Phone5");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone6",
                table: "consumer_enrichments",
                column: "Phone6");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone7",
                table: "consumer_enrichments",
                column: "Phone7");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone8",
                table: "consumer_enrichments",
                column: "Phone8");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone9",
                table: "consumer_enrichments",
                column: "Phone9");

            migrationBuilder.CreateIndex(
                name: "IX_consumer_enrichments_Phone10",
                table: "consumer_enrichments",
                column: "Phone10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "Phone9",
                table: "consumer_enrichments");
        }
    }
}
