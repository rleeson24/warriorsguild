using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class AddSeeHowLink : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<string>(
                name: "SeeHowLink",
                table: "RingRequirements",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "SeeHowLink",
                table: "RankRequirements",
                nullable: true );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "SeeHowLink",
                table: "RingRequirements" );

            migrationBuilder.DropColumn(
                name: "SeeHowLink",
                table: "RankRequirements" );
        }
    }
}
