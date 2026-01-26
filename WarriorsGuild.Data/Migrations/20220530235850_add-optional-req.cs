using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class addoptionalreq : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<bool>(
                name: "Optional",
                table: "RankRequirements",
                nullable: false,
                defaultValue: false );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "Optional",
                table: "RankRequirements" );
        }
    }
}
