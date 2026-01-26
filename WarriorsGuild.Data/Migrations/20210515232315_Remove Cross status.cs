using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class RemoveCrossstatus : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.CreateTable(
                name: "CrossStatuses",
                columns: table => new
                {
                    Id = table.Column<int>( type: "int", nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" ),
                    Completed = table.Column<DateTime>( type: "datetime2", nullable: false ),
                    Confirmed = table.Column<DateTime>( type: "datetime2", nullable: true ),
                    CrossId = table.Column<Guid>( type: "uniqueidentifier", nullable: false ),
                    UserId = table.Column<Guid>( type: "uniqueidentifier", nullable: false ),
                    Voided = table.Column<DateTime>( type: "datetime2", nullable: true )
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
    }
}
