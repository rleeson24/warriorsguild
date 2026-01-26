using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class Updatecrossmodels : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropTable(
                name: "CrossStatuses" );

            migrationBuilder.DropColumn(
                name: "SubmittedTs",
                table: "RankApprovals" );

            migrationBuilder.CreateTable(
                name: "CrossDayAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    CrossId = table.Column<Guid>( nullable: false ),
                    DayId = table.Column<Guid>( nullable: false ),
                    QuestionId = table.Column<Guid>( nullable: false ),
                    Answer = table.Column<string>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_CrossDayAnswers", x => x.Id );
                } );

            migrationBuilder.CreateTable(
                name: "CrossDays",
                columns: table => new
                {
                    DayId = table.Column<Guid>( nullable: false ),
                    CrossId = table.Column<Guid>( nullable: false ),
                    Weight = table.Column<int>( nullable: false ),
                    Passage = table.Column<string>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_CrossDays", x => x.DayId );
                } );

            migrationBuilder.CreateTable(
                name: "PinnedCrosses",
                columns: table => new
                {
                    UserId = table.Column<Guid>( nullable: false ),
                    CrossId = table.Column<Guid>( nullable: false ),
                    Id = table.Column<int>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_PinnedCrosses", x => new { x.UserId, x.CrossId } );
                    table.ForeignKey(
                        name: "FK_PinnedCrosses_Crosses_CrossId",
                        column: x => x.CrossId,
                        principalTable: "Crosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "CrossApprovals",
                columns: table => new
                {
                    Id = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" ),
                    CrossId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    DayId = table.Column<Guid>( nullable: false ),
                    PercentComplete = table.Column<int>( nullable: false ),
                    CompletedAt = table.Column<DateTime>( nullable: false ),
                    ApprovedAt = table.Column<DateTime>( nullable: true ),
                    ReturnedTs = table.Column<DateTime>( nullable: true ),
                    RecalledByWarriorTs = table.Column<DateTime>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_CrossApprovals", x => x.Id );
                    table.ForeignKey(
                        name: "FK_CrossApprovals_Crosses_CrossId",
                        column: x => x.CrossId,
                        principalTable: "Crosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                    table.ForeignKey(
                        name: "FK_CrossApprovals_CrossDays_DayId",
                        column: x => x.DayId,
                        principalTable: "CrossDays",
                        principalColumn: "DayId",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateIndex(
                name: "IX_CrossApprovals_CrossId",
                table: "CrossApprovals",
                column: "CrossId" );

            migrationBuilder.CreateIndex(
                name: "IX_CrossApprovals_DayId",
                table: "CrossApprovals",
                column: "DayId" );

            migrationBuilder.CreateIndex(
                name: "IX_PinnedCrosses_CrossId",
                table: "PinnedCrosses",
                column: "CrossId" );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropTable(
                name: "CrossApprovals" );

            migrationBuilder.DropTable(
                name: "CrossDayAnswers" );

            migrationBuilder.DropTable(
                name: "PinnedCrosses" );

            migrationBuilder.DropTable(
                name: "CrossDays" );

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedTs",
                table: "RankApprovals",
                type: "datetime2",
                nullable: true );

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
