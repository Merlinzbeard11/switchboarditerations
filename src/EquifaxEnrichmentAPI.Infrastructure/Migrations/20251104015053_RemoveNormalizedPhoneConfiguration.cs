using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquifaxEnrichmentAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNormalizedPhoneConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_consumer_enrichments_normalized_phone_unique",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "addresses_json",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "financial_json",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "normalized_phone",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "personal_info_json",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "phones_json",
                table: "consumer_enrichments");

            migrationBuilder.RenameColumn(
                name: "Phone9",
                table: "consumer_enrichments",
                newName: "zip_9");

            migrationBuilder.RenameColumn(
                name: "Phone8",
                table: "consumer_enrichments",
                newName: "zip_8");

            migrationBuilder.RenameColumn(
                name: "Phone7",
                table: "consumer_enrichments",
                newName: "zip_7");

            migrationBuilder.RenameColumn(
                name: "Phone6",
                table: "consumer_enrichments",
                newName: "zip_6");

            migrationBuilder.RenameColumn(
                name: "Phone5",
                table: "consumer_enrichments",
                newName: "zip_5");

            migrationBuilder.RenameColumn(
                name: "Phone4",
                table: "consumer_enrichments",
                newName: "zip_4");

            migrationBuilder.RenameColumn(
                name: "Phone3",
                table: "consumer_enrichments",
                newName: "zip_3");

            migrationBuilder.RenameColumn(
                name: "Phone2",
                table: "consumer_enrichments",
                newName: "zip_2");

            migrationBuilder.RenameColumn(
                name: "Phone10",
                table: "consumer_enrichments",
                newName: "zip_10");

            migrationBuilder.RenameColumn(
                name: "Phone1",
                table: "consumer_enrichments",
                newName: "zip_1");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "data_freshness_date",
                table: "consumer_enrichments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "address_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "affluence_index",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "age",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_first_name_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_first_name_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_first_name_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_first_name_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_first_name_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_last_name_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_last_name_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_last_name_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_last_name_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_last_name_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_middle_name_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_middle_name_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_middle_name_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_middle_name_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_middle_name_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_name_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_name_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_name_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_name_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_name_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_prefix_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_prefix_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_prefix_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_prefix_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_prefix_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_suffix_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_suffix_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_suffix_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_suffix_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternate_suffix_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "auto_in_market_propensity_score",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "automotive_response_intent_indicator",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "balance_auto_finance_loan_accounts",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carrier_route_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_of_birth",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deceased",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_code_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_point_validation_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "economiccohortscode",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_11",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_12",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_13",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_14",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_15",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "financialdurabilityindex",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "financialdurabilityscore",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fips_code_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_seen_date_primary_name",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "house_number_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idfa1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idfa2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idfa3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idfa4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idfa5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "income360_complete",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "income360_non_salary",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "income360_salary",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ipaddress1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ipaddress2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_11",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_12",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_13",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_14",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_15",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_email_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_phone_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_phone_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_phone_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_phone_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_phone_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_seen_date_primary_name",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marketing_email_flag",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "meta_revision",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mobile_phone_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mobile_phone_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "percent_balance_to_high_auto_finance_credit",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "post_direction_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "predirectional_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prefix",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "spending_power",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_abbreviation_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_suffix_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suffix",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_date_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_number_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit_type_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uuid",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vaa",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vac",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vad",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vae",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vaf",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vag",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vah",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vai",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vaj",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vak",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "val",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vam",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "van",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vantage_score_neighborhood_risk_score",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vao",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vap",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vaq",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vas",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vat",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vau",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vav",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vaw",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vax",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vay",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vaz",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vba",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbb",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbc",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbd",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbe",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbf",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbg",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbh",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbi",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbj",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbk",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbl",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbm",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbn",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbo",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbp",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbq",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbr",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbs",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbt",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbu",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbv",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbw",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbx",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vby",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vbz",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vc",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vca",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcb",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcc",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcd",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vce",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcf",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcg",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vch",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vci",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcj",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vck",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcl",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcm",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcn",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vco",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcp",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcq",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcr",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcs",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vct",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcu",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcv",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcw",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcx",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcy",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vcz",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vda",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdb",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdc",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdd",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vde",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdf",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdg",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdh",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdi",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdj",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdk",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdl",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdm",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdn",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdo",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdp",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdq",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vdr",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vds",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vf",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vg",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vh",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vj",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vk",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vl",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vm",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vn",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vo",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vp",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vq",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vr",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vs",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vt",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "vy",
                table: "consumer_enrichments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_1",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_10",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_2",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_3",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_4",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_5",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_6",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_7",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_8",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "z4_type_9",
                table: "consumer_enrichments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_consumer_enrichments_mobile_phone_1",
                table: "consumer_enrichments",
                column: "mobile_phone_1");

            migrationBuilder.CreateIndex(
                name: "ix_consumer_enrichments_phone_1",
                table: "consumer_enrichments",
                column: "phone_1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_consumer_enrichments_mobile_phone_1",
                table: "consumer_enrichments");

            migrationBuilder.DropIndex(
                name: "ix_consumer_enrichments_phone_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "address_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "affluence_index",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "age",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_first_name_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_first_name_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_first_name_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_first_name_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_first_name_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_last_name_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_last_name_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_last_name_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_last_name_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_last_name_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_middle_name_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_middle_name_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_middle_name_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_middle_name_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_middle_name_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_name_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_name_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_name_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_name_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_name_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_prefix_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_prefix_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_prefix_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_prefix_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_prefix_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_suffix_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_suffix_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_suffix_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_suffix_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "alternate_suffix_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "auto_in_market_propensity_score",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "automotive_response_intent_indicator",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "balance_auto_finance_loan_accounts",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "carrier_route_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "city_name_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "deceased",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_code_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "delivery_point_validation_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "economiccohortscode",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_11",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_12",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_13",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_14",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_15",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "email_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "financialdurabilityindex",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "financialdurabilityscore",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "fips_code_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "first_seen_date_primary_name",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "house_number_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "idfa1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "idfa2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "idfa3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "idfa4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "idfa5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "income360_complete",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "income360_non_salary",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "income360_salary",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "ipaddress1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "ipaddress2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_11",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_12",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_13",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_14",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_15",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_email_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_phone_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_phone_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_phone_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_phone_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_phone_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "last_seen_date_primary_name",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "marketing_email_flag",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "meta_revision",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "middle_name",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "mobile_phone_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "mobile_phone_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "percent_balance_to_high_auto_finance_credit",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "phone_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "phone_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "phone_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "phone_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "phone_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "post_direction_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "predirectional_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "prefix",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "spending_power",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "state_abbreviation_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_name_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "street_suffix_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "suffix",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "transaction_date_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_number_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "unit_type_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "uuid",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vaa",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vac",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vad",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vae",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vaf",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vag",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vah",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vai",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vaj",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vak",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "val",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vam",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "van",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vantage_score_neighborhood_risk_score",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vao",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vap",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vaq",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vas",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vat",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vau",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vav",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vaw",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vax",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vay",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vaz",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vba",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbb",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbc",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbd",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbe",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbf",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbg",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbh",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbi",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbj",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbk",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbl",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbm",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbn",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbo",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbp",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbq",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbr",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbs",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbt",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbu",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbv",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbw",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbx",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vby",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vbz",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vc",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vca",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcb",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcc",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcd",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vce",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcf",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcg",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vch",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vci",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcj",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vck",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcl",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcm",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcn",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vco",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcp",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcq",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcr",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcs",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vct",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcu",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcv",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcw",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcx",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcy",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vcz",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vda",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdb",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdc",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdd",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vde",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdf",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdg",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdh",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdi",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdj",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdk",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdl",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdm",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdn",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdo",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdp",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdq",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vdr",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vds",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vf",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vg",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vh",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vj",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vk",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vl",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vm",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vn",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vo",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vp",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vq",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vr",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vs",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vt",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "vy",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_9",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_1",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_10",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_2",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_3",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_4",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_5",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_6",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_7",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_8",
                table: "consumer_enrichments");

            migrationBuilder.DropColumn(
                name: "z4_type_9",
                table: "consumer_enrichments");

            migrationBuilder.RenameColumn(
                name: "zip_9",
                table: "consumer_enrichments",
                newName: "Phone9");

            migrationBuilder.RenameColumn(
                name: "zip_8",
                table: "consumer_enrichments",
                newName: "Phone8");

            migrationBuilder.RenameColumn(
                name: "zip_7",
                table: "consumer_enrichments",
                newName: "Phone7");

            migrationBuilder.RenameColumn(
                name: "zip_6",
                table: "consumer_enrichments",
                newName: "Phone6");

            migrationBuilder.RenameColumn(
                name: "zip_5",
                table: "consumer_enrichments",
                newName: "Phone5");

            migrationBuilder.RenameColumn(
                name: "zip_4",
                table: "consumer_enrichments",
                newName: "Phone4");

            migrationBuilder.RenameColumn(
                name: "zip_3",
                table: "consumer_enrichments",
                newName: "Phone3");

            migrationBuilder.RenameColumn(
                name: "zip_2",
                table: "consumer_enrichments",
                newName: "Phone2");

            migrationBuilder.RenameColumn(
                name: "zip_10",
                table: "consumer_enrichments",
                newName: "Phone10");

            migrationBuilder.RenameColumn(
                name: "zip_1",
                table: "consumer_enrichments",
                newName: "Phone1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_freshness_date",
                table: "consumer_enrichments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "addresses_json",
                table: "consumer_enrichments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "financial_json",
                table: "consumer_enrichments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "normalized_phone",
                table: "consumer_enrichments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "personal_info_json",
                table: "consumer_enrichments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phones_json",
                table: "consumer_enrichments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_consumer_enrichments_normalized_phone_unique",
                table: "consumer_enrichments",
                column: "normalized_phone",
                unique: true);
        }
    }
}
