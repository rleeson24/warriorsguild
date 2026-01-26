using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class AddIsCheckpointtoCrossDay : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCheckpoint",
                table: "CrossDays",
                nullable: false,
                defaultValue: false );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "IsCheckpoint",
                table: "CrossDays" );
        }
    }
}
