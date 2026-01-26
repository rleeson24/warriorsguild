using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class addrankreqcrosses : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.CreateTable(
                name: "RankCrosses",
                columns: table => new
                {
                    Key = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" ),
                    RankId = table.Column<Guid>( nullable: false ),
                    RankRequirementId = table.Column<Guid>( nullable: false ),
                    CrossId = table.Column<Guid>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RankCrosses", x => x.Key );
                    table.ForeignKey(
                        name: "FK_RankCrosses_Crosses_CrossId",
                        column: x => x.CrossId,
                        principalTable: "Crosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateIndex(
                name: "IX_RankCrosses_CrossId",
                table: "RankCrosses",
                column: "CrossId" );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropTable(
                name: "RankCrosses" );
        }
    }
}
