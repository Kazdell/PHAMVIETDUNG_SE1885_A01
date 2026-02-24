using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PHAMVIETDUNG_SE1885_A02_BE.Migrations
{
  /// <inheritdoc />
  public partial class AddViewCount : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<int>(
          name: "ViewCount",
          table: "NewsArticle",
          type: "int",
          nullable: false,
          defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "ViewCount",
          table: "NewsArticle");
    }
  }
}
