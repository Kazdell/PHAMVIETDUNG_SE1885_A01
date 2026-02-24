using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PHAMVIETDUNG_SE1885_A02_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewsImage",
                table: "NewsArticle",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsImage",
                table: "NewsArticle");
        }
    }
}
