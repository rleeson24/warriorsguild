using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class AddReturnedBy : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReturnedBy",
                table: "RankApprovals",
                nullable: false,
                defaultValue: new Guid( "00000000-0000-0000-0000-000000000000" ) );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "ReturnedBy",
                table: "RankApprovals" );
        }
    }
}
