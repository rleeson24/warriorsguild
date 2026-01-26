using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class MakeDayIdNullableonApprovalRec : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrossApprovals_CrossDays_DayId",
                table: "CrossApprovals" );

            migrationBuilder.AlterColumn<Guid>(
                name: "DayId",
                table: "CrossApprovals",
                nullable: true,
                oldClrType: typeof( Guid ),
                oldType: "uniqueidentifier" );

            migrationBuilder.AddForeignKey(
                name: "FK_CrossApprovals_CrossDays_DayId",
                table: "CrossApprovals",
                column: "DayId",
                principalTable: "CrossDays",
                principalColumn: "DayId",
                onDelete: ReferentialAction.Restrict );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrossApprovals_CrossDays_DayId",
                table: "CrossApprovals" );

            migrationBuilder.AlterColumn<Guid>(
                name: "DayId",
                table: "CrossApprovals",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof( Guid ),
                oldNullable: true );

            migrationBuilder.AddForeignKey(
                name: "FK_CrossApprovals_CrossDays_DayId",
                table: "CrossApprovals",
                column: "DayId",
                principalTable: "CrossDays",
                principalColumn: "DayId",
                onDelete: ReferentialAction.Cascade );
        }
    }
}
