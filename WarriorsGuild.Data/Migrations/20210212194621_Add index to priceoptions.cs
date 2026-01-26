using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class Addindextopriceoptions : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.CreateIndex(
                name: "IX_PriceOptions_Id_ModifiedDate",
                table: "PriceOptions",
                columns: new[] { "Id", "ModifiedDate" } );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceOptions_Id_ModifiedDate",
                table: "PriceOptions" );
        }
    }
}
