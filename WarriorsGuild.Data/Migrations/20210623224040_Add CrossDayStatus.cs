using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class AddCrossDayStatus : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.CreateTable(
                name: "CrossDayStatuses",
                columns: table => new
                {
                    Id = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" ),
                    DayId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    CompletedAt = table.Column<DateTime>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_CrossDayStatuses", x => x.Id );
                } );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropTable(
                name: "CrossDayStatuses" );
        }
    }
}
