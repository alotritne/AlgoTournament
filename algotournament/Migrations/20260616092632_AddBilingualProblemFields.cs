using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace algotournament.Migrations
{
    /// <inheritdoc />
    public partial class AddBilingualProblemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConstraintsEn",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ConstraintsVi",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnglishTranslatedAt",
                table: "Problems",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExplanationEn",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ExplanationVi",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InputDescriptionEn",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InputDescriptionVi",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnglishTranslated",
                table: "Problems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OutputDescriptionEn",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OutputDescriptionVi",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StatementEn",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StatementVi",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TitleVi",
                table: "Problems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConstraintsEn",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "ConstraintsVi",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "EnglishTranslatedAt",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "ExplanationEn",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "ExplanationVi",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "InputDescriptionEn",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "InputDescriptionVi",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "IsEnglishTranslated",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "OutputDescriptionEn",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "OutputDescriptionVi",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "StatementEn",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "StatementVi",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "TitleVi",
                table: "Problems");
        }
    }
}
