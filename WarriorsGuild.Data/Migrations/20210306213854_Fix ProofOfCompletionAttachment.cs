using Microsoft.EntityFrameworkCore.Migrations;

namespace WarriorsGuild.Data.Migrations
{
    public partial class FixProofOfCompletionAttachment : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProofOfCompletionAttachments",
                table: "ProofOfCompletionAttachments" );

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProofOfCompletionAttachments",
                table: "ProofOfCompletionAttachments",
                column: "Id" );

            migrationBuilder.CreateIndex(
                name: "IX_ProofOfCompletionAttachments_RequirementId_UserId",
                table: "ProofOfCompletionAttachments",
                columns: new[] { "RequirementId", "UserId" } );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProofOfCompletionAttachments",
                table: "ProofOfCompletionAttachments" );

            migrationBuilder.DropIndex(
                name: "IX_ProofOfCompletionAttachments_RequirementId_UserId",
                table: "ProofOfCompletionAttachments" );

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProofOfCompletionAttachments",
                table: "ProofOfCompletionAttachments",
                columns: new[] { "RequirementId", "UserId" } );
        }
    }
}
