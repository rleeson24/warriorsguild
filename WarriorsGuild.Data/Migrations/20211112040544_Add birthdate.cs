using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class Addbirthdate : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: new DateTime( 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified ) );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AspNetUsers" );
        }
    }
}
