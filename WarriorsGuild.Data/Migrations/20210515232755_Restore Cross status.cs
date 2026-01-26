using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class RestoreCrossstatus : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.CreateTable(
                name: "CrossStatuses",
                columns: table => new
                {
                    Id = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" ),
                    CrossId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    Completed = table.Column<DateTime>( nullable: false ),
                    Confirmed = table.Column<DateTime>( nullable: true ),
                    Voided = table.Column<DateTime>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_CrossStatuses", x => x.Id );
                    table.ForeignKey(
                        name: "FK_CrossStatuses_Crosses_CrossId",
                        column: x => x.CrossId,
                        principalTable: "Crosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateIndex(
                name: "IX_CrossStatuses_CrossId",
                table: "CrossStatuses",
                column: "CrossId" );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropTable(
                name: "CrossStatuses" );
        }
    }
}
