using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class AddSignedCoventants : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.CreateTable(
                name: "SignedCovenants",
                columns: table => new
                {
                    SignedBy = table.Column<Guid>( nullable: false ),
                    SignedAt = table.Column<DateTime>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_SignedCovenants", x => new { x.SignedAt, x.SignedBy } );
                } );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropTable(
                name: "SignedCovenants" );
        }
    }
}
