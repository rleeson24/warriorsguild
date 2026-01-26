using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class Addcrossorders : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "CrossQuestions",
                nullable: false,
                defaultValue: 0 );

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "CrossDays",
                nullable: false,
                defaultValue: 0 );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "CrossQuestions" );

            migrationBuilder.DropColumn(
                name: "Index",
                table: "CrossDays" );
        }
    }
}
