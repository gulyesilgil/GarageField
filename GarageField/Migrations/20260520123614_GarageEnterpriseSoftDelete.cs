using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageField.Migrations
{
    /// <inheritdoc />
    public partial class GarageEnterpriseSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "inspections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "inspections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "inspection_files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "inspection_files",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "inspection_files",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "inspections");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "inspections");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "inspection_files");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "inspection_files");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "inspection_files");
        }
    }
}
