using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class Addimagefileextension : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageExtension",
                table: "Rings",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "ImageExtension",
                table: "Ranks",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "ImageExtension",
                table: "Crosses",
                nullable: true );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "ImageExtension",
                table: "Rings" );

            migrationBuilder.DropColumn(
                name: "ImageExtension",
                table: "Ranks" );

            migrationBuilder.DropColumn(
                name: "ImageExtension",
                table: "Crosses" );
        }
    }
}
