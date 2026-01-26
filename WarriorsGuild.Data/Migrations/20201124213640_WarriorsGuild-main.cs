using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WarriorsGuild.Data.Migrations
{
    public partial class WarriorsGuildmain : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof( string ),
                oldType: "nvarchar(128)",
                oldMaxLength: 128 );

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof( string ),
                oldType: "nvarchar(128)",
                oldMaxLength: 128 );

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarUserId",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "FavoriteMovie",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "FavoriteVerse",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "Hobbies",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "InterestingFact",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "ShirtSize",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "AspNetUsers",
                nullable: true );

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof( string ),
                oldType: "nvarchar(128)",
                oldMaxLength: 128 );

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof( string ),
                oldType: "nvarchar(128)",
                oldMaxLength: 128 );

            migrationBuilder.CreateTable(
                name: "AddOnPriceOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    CreatedDate = table.Column<DateTime>( nullable: false ),
                    ModifiedDate = table.Column<DateTime>( nullable: false ),
                    Key = table.Column<string>( nullable: true ),
                    Description = table.Column<string>( nullable: false ),
                    Frequency = table.Column<int>( nullable: false ),
                    Charge = table.Column<decimal>( nullable: false ),
                    Currency = table.Column<string>( nullable: true ),
                    Show = table.Column<bool>( nullable: false ),
                    TrialPeriodLength = table.Column<int>( nullable: true ),
                    NumberOfGuardians = table.Column<int>( nullable: false ),
                    NumberOfWarriors = table.Column<int>( nullable: false ),
                    StripePlanId = table.Column<string>( nullable: true ),
                    StripeProductId = table.Column<string>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_AddOnPriceOptions", x => x.Id );
                } );

            migrationBuilder.CreateTable(
                name: "AvatarDetail",
                columns: table => new
                {
                    UserId = table.Column<Guid>( nullable: false ),
                    Data = table.Column<byte[]>( nullable: true ),
                    Extension = table.Column<string>( nullable: true ),
                    ContentType = table.Column<string>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_AvatarDetail", x => x.UserId );
                } );

            migrationBuilder.CreateTable(
                name: "CrossAnswers",
                columns: table => new
                {
                    CrossId = table.Column<Guid>( nullable: false ),
                    CrossQuestionId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    Answer = table.Column<string>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_CrossAnswers", x => new { x.CrossId, x.CrossQuestionId, x.UserId } );
                } );

            migrationBuilder.CreateTable(
                name: "Crosses",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    Description = table.Column<string>( nullable: false ),
                    Name = table.Column<string>( nullable: false ),
                    ImageUploaded = table.Column<DateTime>( nullable: true ),
                    GuideUploaded = table.Column<DateTime>( nullable: true ),
                    GuideFileExtension = table.Column<string>( nullable: true ),
                    Index = table.Column<int>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_Crosses", x => x.Id );
                } );

            migrationBuilder.CreateTable(
                name: "InvitedEmailAddresses",
                columns: table => new
                {
                    EmailAddress = table.Column<string>( nullable: false ),
                    InvitedBy = table.Column<Guid>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_InvitedEmailAddresses", x => x.EmailAddress );
                } );

            migrationBuilder.CreateTable(
                name: "MailingList",
                columns: table => new
                {
                    EmailAddress = table.Column<string>( nullable: false ),
                    Subscribed = table.Column<bool>( nullable: false ),
                    FreeReportSent = table.Column<bool>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_MailingList", x => x.EmailAddress );
                } );

            migrationBuilder.CreateTable(
                name: "ProofOfCompletionAttachments",
                columns: table => new
                {
                    RequirementId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    Id = table.Column<Guid>( nullable: false ),
                    StorageKey = table.Column<string>( nullable: false ),
                    FileExtension = table.Column<string>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_ProofOfCompletionAttachments", x => new { x.RequirementId, x.UserId } );
                } );

            migrationBuilder.CreateTable(
                name: "Ranks",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    Name = table.Column<string>( nullable: false ),
                    Description = table.Column<string>( nullable: true ),
                    ImageUploaded = table.Column<DateTime>( nullable: true ),
                    GuideUploaded = table.Column<DateTime>( nullable: true ),
                    GuideFileExtension = table.Column<string>( nullable: true ),
                    Index = table.Column<int>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_Ranks", x => x.Id );
                } );

            migrationBuilder.CreateTable(
                name: "Rings",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    Name = table.Column<string>( nullable: false ),
                    Description = table.Column<string>( nullable: true ),
                    Type = table.Column<int>( nullable: false ),
                    ImageUploaded = table.Column<DateTime>( nullable: true ),
                    GuideUploaded = table.Column<DateTime>( nullable: true ),
                    GuideFileExtension = table.Column<string>( nullable: true ),
                    Index = table.Column<int>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_Rings", x => x.Id );
                } );

            migrationBuilder.CreateTable(
                name: "SingleUseFileDownloadKey",
                columns: table => new
                {
                    Key = table.Column<Guid>( nullable: false ),
                    AttachmentId = table.Column<Guid>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_SingleUseFileDownloadKey", x => x.Key );
                } );

            migrationBuilder.CreateTable(
                name: "StripeWebhookMessages",
                columns: table => new
                {
                    Id = table.Column<string>( nullable: false ),
                    StripeEvent = table.Column<string>( nullable: true ),
                    EventType = table.Column<string>( nullable: true ),
                    Received = table.Column<DateTime>( nullable: false ),
                    LiveMode = table.Column<bool>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_StripeWebhookMessages", x => x.Id );
                } );

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    UserId = table.Column<Guid>( nullable: false ),
                    SubscriptionId = table.Column<Guid>( nullable: false ),
                    BillingAgreementId = table.Column<Guid>( nullable: false ),
                    Role = table.Column<int>( nullable: false ),
                    IsPayingParty = table.Column<bool>( nullable: false ),
                    Revised = table.Column<DateTime>( nullable: true ),
                    RevisedBy = table.Column<Guid>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_UserSubscriptions", x => new { x.UserId, x.SubscriptionId, x.BillingAgreementId } );
                } );

            migrationBuilder.CreateTable(
                name: "PriceOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    CreatedDate = table.Column<DateTime>( nullable: false ),
                    ModifiedDate = table.Column<DateTime>( nullable: false ),
                    Key = table.Column<string>( nullable: true ),
                    Description = table.Column<string>( nullable: false ),
                    Frequency = table.Column<int>( nullable: false ),
                    Charge = table.Column<decimal>( nullable: false ),
                    Currency = table.Column<string>( nullable: true ),
                    Show = table.Column<bool>( nullable: false ),
                    TrialPeriodLength = table.Column<int>( nullable: true ),
                    NumberOfGuardians = table.Column<int>( nullable: false ),
                    NumberOfWarriors = table.Column<int>( nullable: false ),
                    StripePlanId = table.Column<string>( nullable: true ),
                    StripeProductId = table.Column<string>( nullable: true ),
                    SetupFee = table.Column<decimal>( nullable: false ),
                    HasTrialPeriod = table.Column<bool>( nullable: false ),
                    Voided = table.Column<DateTime>( nullable: true ),
                    AdditionalGuardianPlanId = table.Column<Guid>( nullable: true ),
                    AdditionalWarriorPlanId = table.Column<Guid>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_PriceOptions", x => x.Id );
                    table.ForeignKey(
                        name: "FK_PriceOptions_AddOnPriceOptions_AdditionalGuardianPlanId",
                        column: x => x.AdditionalGuardianPlanId,
                        principalTable: "AddOnPriceOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict );
                    table.ForeignKey(
                        name: "FK_PriceOptions_AddOnPriceOptions_AdditionalWarriorPlanId",
                        column: x => x.AdditionalWarriorPlanId,
                        principalTable: "AddOnPriceOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict );
                } );

            migrationBuilder.CreateTable(
                name: "CrossQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    Text = table.Column<string>( nullable: false ),
                    CrossId = table.Column<Guid>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_CrossQuestions", x => x.Id );
                    table.ForeignKey(
                        name: "FK_CrossQuestions_Crosses_CrossId",
                        column: x => x.CrossId,
                        principalTable: "Crosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RankStatusCrosses",
                columns: table => new
                {
                    RankId = table.Column<Guid>( nullable: false ),
                    RankRequirementId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    CrossId = table.Column<Guid>( nullable: false ),
                    Key = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RankStatusCrosses", x => new { x.RankId, x.RankRequirementId, x.UserId, x.CrossId } );
                    table.ForeignKey(
                        name: "FK_RankStatusCrosses_Crosses_CrossId",
                        column: x => x.CrossId,
                        principalTable: "Crosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RankApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    RankId = table.Column<Guid>( nullable: false ),
                    PercentComplete = table.Column<int>( nullable: false ),
                    CompletedAt = table.Column<DateTime>( nullable: false ),
                    ApprovedAt = table.Column<DateTime>( nullable: true ),
                    RecalledByWarriorTs = table.Column<DateTime>( nullable: true ),
                    ReturnedTs = table.Column<DateTime>( nullable: true ),
                    SubmittedTs = table.Column<DateTime>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RankApprovals", x => x.Id );
                    table.ForeignKey(
                        name: "FK_RankApprovals_Ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "Ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RankRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    RankId = table.Column<Guid>( nullable: false ),
                    ActionToComplete = table.Column<string>( nullable: false ),
                    Index = table.Column<int>( nullable: false ),
                    Weight = table.Column<int>( nullable: false ),
                    RequireAttachment = table.Column<bool>( nullable: false ),
                    RequireRing = table.Column<bool>( nullable: false ),
                    RequireCross = table.Column<bool>( nullable: false ),
                    RequiredRingType = table.Column<int>( nullable: true ),
                    RequiredRingCount = table.Column<int>( nullable: true ),
                    RequiredCrossCount = table.Column<int>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RankRequirements", x => x.Id );
                    table.ForeignKey(
                        name: "FK_RankRequirements_Ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "Ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RankStatuses",
                columns: table => new
                {
                    Id = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" ),
                    RankId = table.Column<Guid>( nullable: false ),
                    RankRequirementId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    WarriorCompleted = table.Column<DateTime>( nullable: false ),
                    GuardianCompleted = table.Column<DateTime>( nullable: true ),
                    ReturnedTs = table.Column<DateTime>( nullable: true ),
                    RecalledByWarriorTs = table.Column<DateTime>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RankStatuses", x => x.Id );
                    table.ForeignKey(
                        name: "FK_RankStatuses_Ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "Ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "PinnedRings",
                columns: table => new
                {
                    UserId = table.Column<Guid>( nullable: false ),
                    RingId = table.Column<Guid>( nullable: false ),
                    Id = table.Column<int>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_PinnedRings", x => new { x.UserId, x.RingId } );
                    table.ForeignKey(
                        name: "FK_PinnedRings_Rings_RingId",
                        column: x => x.RingId,
                        principalTable: "Rings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RankStatusRings",
                columns: table => new
                {
                    RankId = table.Column<Guid>( nullable: false ),
                    RankRequirementId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    RingId = table.Column<Guid>( nullable: false ),
                    Key = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RankStatusRings", x => new { x.RankId, x.RankRequirementId, x.UserId, x.RingId } );
                    table.ForeignKey(
                        name: "FK_RankStatusRings_Rings_RingId",
                        column: x => x.RingId,
                        principalTable: "Rings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RingApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    RingId = table.Column<Guid>( nullable: false ),
                    CompletedAt = table.Column<DateTime>( nullable: false ),
                    ApprovedAt = table.Column<DateTime>( nullable: true ),
                    RecalledByWarriorTs = table.Column<DateTime>( nullable: true ),
                    ReturnedTs = table.Column<DateTime>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RingApprovals", x => x.Id );
                    table.ForeignKey(
                        name: "FK_RingApprovals_Rings_RingId",
                        column: x => x.RingId,
                        principalTable: "Rings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RingRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    RingId = table.Column<Guid>( nullable: false ),
                    ActionToComplete = table.Column<string>( nullable: false ),
                    Index = table.Column<int>( nullable: false ),
                    Weight = table.Column<int>( nullable: false ),
                    RequireAttachment = table.Column<bool>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RingRequirements", x => x.Id );
                    table.ForeignKey(
                        name: "FK_RingRequirements_Rings_RingId",
                        column: x => x.RingId,
                        principalTable: "Rings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "RingStatuses",
                columns: table => new
                {
                    Id = table.Column<int>( nullable: false )
                        .Annotation( "SqlServer:Identity", "1, 1" ),
                    RingId = table.Column<Guid>( nullable: false ),
                    RingRequirementId = table.Column<Guid>( nullable: false ),
                    UserId = table.Column<Guid>( nullable: false ),
                    WarriorCompleted = table.Column<DateTime>( nullable: false ),
                    GuardianCompleted = table.Column<DateTime>( nullable: true ),
                    ReturnedTs = table.Column<DateTime>( nullable: true ),
                    RecalledByWarriorTs = table.Column<DateTime>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_RingStatuses", x => x.Id );
                    table.ForeignKey(
                        name: "FK_RingStatuses_Rings_RingId",
                        column: x => x.RingId,
                        principalTable: "Rings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "BillingAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    Created = table.Column<DateTime>( nullable: false ),
                    LastPaid = table.Column<DateTime>( nullable: false ),
                    NextPaymentDue = table.Column<DateTime>( nullable: false ),
                    Cancelled = table.Column<DateTime>( nullable: true ),
                    Status = table.Column<int>( nullable: false ),
                    AdditionalGuardians = table.Column<int>( nullable: false ),
                    AdditionalWarriors = table.Column<int>( nullable: false ),
                    PaymentMethod = table.Column<int>( nullable: false ),
                    StripeSubscriptionId = table.Column<string>( nullable: true ),
                    PriceOptionId = table.Column<Guid>( nullable: true )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_BillingAgreements", x => x.Id );
                    table.ForeignKey(
                        name: "FK_BillingAgreements_PriceOptions_PriceOptionId",
                        column: x => x.PriceOptionId,
                        principalTable: "PriceOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict );
                } );

            migrationBuilder.CreateTable(
                name: "PriceOptionPerks",
                columns: table => new
                {
                    Id = table.Column<Guid>( nullable: false ),
                    PriceOptionId = table.Column<Guid>( nullable: false ),
                    Quantity = table.Column<int>( nullable: true ),
                    Description = table.Column<string>( nullable: false ),
                    Index = table.Column<int>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_PriceOptionPerks", x => x.Id );
                    table.ForeignKey(
                        name: "FK_PriceOptionPerks_PriceOptions_PriceOptionId",
                        column: x => x.PriceOptionId,
                        principalTable: "PriceOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateTable(
                name: "SubscriptionBillingAgreements",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>( nullable: false ),
                    BillingAgreementId = table.Column<Guid>( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey( "PK_SubscriptionBillingAgreements", x => new { x.SubscriptionId, x.BillingAgreementId } );
                    table.ForeignKey(
                        name: "FK_SubscriptionBillingAgreements_BillingAgreements_BillingAgreementId",
                        column: x => x.BillingAgreementId,
                        principalTable: "BillingAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                } );

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ApplicationUserId",
                table: "AspNetUsers",
                column: "ApplicationUserId" );

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AvatarUserId",
                table: "AspNetUsers",
                column: "AvatarUserId" );

            migrationBuilder.CreateIndex(
                name: "IX_BillingAgreements_PriceOptionId",
                table: "BillingAgreements",
                column: "PriceOptionId" );

            migrationBuilder.CreateIndex(
                name: "IX_CrossQuestions_CrossId",
                table: "CrossQuestions",
                column: "CrossId" );

            migrationBuilder.CreateIndex(
                name: "IX_PinnedRings_RingId",
                table: "PinnedRings",
                column: "RingId" );

            migrationBuilder.CreateIndex(
                name: "IX_PriceOptionPerks_PriceOptionId",
                table: "PriceOptionPerks",
                column: "PriceOptionId" );

            migrationBuilder.CreateIndex(
                name: "IX_PriceOptions_AdditionalGuardianPlanId",
                table: "PriceOptions",
                column: "AdditionalGuardianPlanId" );

            migrationBuilder.CreateIndex(
                name: "IX_PriceOptions_AdditionalWarriorPlanId",
                table: "PriceOptions",
                column: "AdditionalWarriorPlanId" );

            migrationBuilder.CreateIndex(
                name: "IX_RankApprovals_RankId",
                table: "RankApprovals",
                column: "RankId" );

            migrationBuilder.CreateIndex(
                name: "IX_RankRequirements_RankId",
                table: "RankRequirements",
                column: "RankId" );

            migrationBuilder.CreateIndex(
                name: "IX_RankStatusCrosses_CrossId",
                table: "RankStatusCrosses",
                column: "CrossId" );

            migrationBuilder.CreateIndex(
                name: "IX_RankStatuses_RankId",
                table: "RankStatuses",
                column: "RankId" );

            migrationBuilder.CreateIndex(
                name: "IX_RankStatusRings_RingId",
                table: "RankStatusRings",
                column: "RingId" );

            migrationBuilder.CreateIndex(
                name: "IX_RingApprovals_RingId",
                table: "RingApprovals",
                column: "RingId" );

            migrationBuilder.CreateIndex(
                name: "IX_RingRequirements_RingId",
                table: "RingRequirements",
                column: "RingId" );

            migrationBuilder.CreateIndex(
                name: "IX_RingStatuses_RingId",
                table: "RingStatuses",
                column: "RingId" );

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionBillingAgreements_BillingAgreementId",
                table: "SubscriptionBillingAgreements",
                column: "BillingAgreementId" );

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ApplicationUserId",
                table: "AspNetUsers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict );

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AvatarDetail_AvatarUserId",
                table: "AspNetUsers",
                column: "AvatarUserId",
                principalTable: "AvatarDetail",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ApplicationUserId",
                table: "AspNetUsers" );

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AvatarDetail_AvatarUserId",
                table: "AspNetUsers" );

            migrationBuilder.DropTable(
                name: "AvatarDetail" );

            migrationBuilder.DropTable(
                name: "CrossAnswers" );

            migrationBuilder.DropTable(
                name: "CrossQuestions" );

            migrationBuilder.DropTable(
                name: "InvitedEmailAddresses" );

            migrationBuilder.DropTable(
                name: "MailingList" );

            migrationBuilder.DropTable(
                name: "PinnedRings" );

            migrationBuilder.DropTable(
                name: "PriceOptionPerks" );

            migrationBuilder.DropTable(
                name: "ProofOfCompletionAttachments" );

            migrationBuilder.DropTable(
                name: "RankApprovals" );

            migrationBuilder.DropTable(
                name: "RankRequirements" );

            migrationBuilder.DropTable(
                name: "RankStatusCrosses" );

            migrationBuilder.DropTable(
                name: "RankStatuses" );

            migrationBuilder.DropTable(
                name: "RankStatusRings" );

            migrationBuilder.DropTable(
                name: "RingApprovals" );

            migrationBuilder.DropTable(
                name: "RingRequirements" );

            migrationBuilder.DropTable(
                name: "RingStatuses" );

            migrationBuilder.DropTable(
                name: "SingleUseFileDownloadKey" );

            migrationBuilder.DropTable(
                name: "StripeWebhookMessages" );

            migrationBuilder.DropTable(
                name: "SubscriptionBillingAgreements" );

            migrationBuilder.DropTable(
                name: "UserSubscriptions" );

            migrationBuilder.DropTable(
                name: "Crosses" );

            migrationBuilder.DropTable(
                name: "Ranks" );

            migrationBuilder.DropTable(
                name: "Rings" );

            migrationBuilder.DropTable(
                name: "BillingAgreements" );

            migrationBuilder.DropTable(
                name: "PriceOptions" );

            migrationBuilder.DropTable(
                name: "AddOnPriceOptions" );

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ApplicationUserId",
                table: "AspNetUsers" );

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AvatarUserId",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "AvatarUserId",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "City",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "FavoriteMovie",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "FavoriteVerse",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "Hobbies",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "InterestingFact",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "ShirtSize",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "State",
                table: "AspNetUsers" );

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "AspNetUsers" );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof( string ) );

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof( string ) );

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof( string ) );

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof( string ) );
        }
    }
}
