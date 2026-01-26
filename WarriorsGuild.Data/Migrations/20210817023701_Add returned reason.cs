using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class Addreturnedreason : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnedReason",
                table: "RingApprovals",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "ReturnedReason",
                table: "RankApprovals",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "ReturnedReason",
                table: "CrossApprovals",
                nullable: true );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "ReturnedReason",
                table: "RingApprovals" );

            migrationBuilder.DropColumn(
                name: "ReturnedReason",
                table: "RankApprovals" );

            migrationBuilder.DropColumn(
                name: "ReturnedReason",
                table: "CrossApprovals" );
        }
    }
}
