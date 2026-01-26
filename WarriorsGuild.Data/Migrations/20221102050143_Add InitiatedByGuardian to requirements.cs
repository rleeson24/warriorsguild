using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class AddInitiatedByGuardiantorequirements : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<bool>(
                name: "InitiatedByGuardian",
                table: "RankRequirements",
                nullable: false,
                defaultValue: false );

            migrationBuilder.AddColumn<double>(
                name: "ShowAtPercent",
                table: "RankRequirements",
                nullable: false,
                defaultValue: 0.0 );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "InitiatedByGuardian",
                table: "RankRequirements" );

            migrationBuilder.DropColumn(
                name: "ShowAtPercent",
                table: "RankRequirements" );
        }
    }
}
