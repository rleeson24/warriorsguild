using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Areas.Products;
using WarriorsGuild.Areas.Products.Controllers;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Tests.Areas.Products.Controllers
{
    [TestFixture]
    public class MailingListEntriesControllerTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;
        private Mock<IMailingListProvider> mockMailingListProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private MailingListEntriesController CreateMailingListEntriesController()
        {
            this.mockMailingListProvider = this.mockRepository.Create<IMailingListProvider>();

            return new MailingListEntriesController(
                this.mockMailingListProvider.Object );
        }

        [Test]
        public void GetMailingList_Returns_the_result_from_Provider_GetMailingList()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            
            var mailingList = new MailingListEntry[]
                {
                   _fixture.Build<MailingListEntry>().Create(),
                   _fixture.Build<MailingListEntry>().Create(),
                   _fixture.Build<MailingListEntry>().Create()
                }.AsQueryable();
            mockMailingListProvider.Setup( m => m.GetMailingList() ).Returns( mailingList );

            // Act
            var result = unitUnderTest.GetMailingList();

            // Assert
            Assert.AreEqual( mailingList, result );
        }

        [Test]
        public async Task GetMailingListEntry_When_provider_returns_null_Returns_NotFound()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string id = "asd";
            mockMailingListProvider.Setup( m => m.GetMailingListEntry( id ) ).Returns( Task.FromResult( default( MailingListEntry ) ) );

            // Act
            var result = await unitUnderTest.GetMailingListEntry( id );

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
        }

        [Test]
        public async Task GetMailingListEntry_When_provider_returns_not_null_Returns_Ok()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string id = "asd";
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();
            mockMailingListProvider.Setup( m => m.GetMailingListEntry( id ) ).Returns( Task.FromResult( mailingListEntry ) );

            // Act
            var result = await unitUnderTest.GetMailingListEntry( id );
            var contentResult = result.Result as OkObjectResult;
            // Assert
            Assert.AreSame( mailingListEntry, contentResult.Value );
        }

        [Test]
        public async Task PutMailingListEntry_Given_a_model_error_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            unitUnderTest.ModelState.AddModelError( "EmailAddress", "sdfasd" );
            string id = "dsafasd";
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();

            // Act
            var result = await unitUnderTest.PutMailingListEntry(
                id,
                mailingListEntry );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result );
        }

        [Test]
        public async Task PutMailingListEntry_Given_id_does_not_match_email_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string id = "dsafasd";
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();

            // Act
            var result = await unitUnderTest.PutMailingListEntry(
                id,
                mailingListEntry );

            // Assert
            Assert.IsInstanceOf( typeof( BadRequestResult ), result );
        }

        [Test]
        public async Task PutMailingListEntry_Given_a_conflict_exception_raised_from_Provider_Returns_Conflict()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();
            string id = mailingListEntry.EmailAddress;
            mockMailingListProvider.Setup( m => m.PutMailingListEntry( id, mailingListEntry ) ).Throws<ConflictException>();

            // Act
            var result = await unitUnderTest.PutMailingListEntry(
                id,
                mailingListEntry );

            // Assert
            Assert.IsInstanceOf( typeof( ConflictResult ), result );
        }

        [Test]
        public async Task PutMailingListEntry_Given_successful_process_Returns_NoContent()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();
            string id = mailingListEntry.EmailAddress;
            var savedEntry =_fixture.Build<MailingListEntry>().Create();
            mockMailingListProvider.Setup( m => m.PutMailingListEntry( id, mailingListEntry ) ).Returns( Task.FromResult( savedEntry ) );

            // Act
            var result = await unitUnderTest.PutMailingListEntry(
                id,
                mailingListEntry );

            // Assert
            Assert.IsInstanceOf<NoContentResult>( result );
            Assert.AreEqual( (result as StatusCodeResult).StatusCode, (int)System.Net.HttpStatusCode.NoContent );
        }

        [Test]
        public async Task PostMailingListEntry_Given_a_model_error_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            unitUnderTest.ModelState.AddModelError( "EmailAddress", "sdfasd" );
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();

            // Act
            var result = await unitUnderTest.PostMailingListEntry( mailingListEntry );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result.Result );
        }

        [Test]
        public async Task PostMailingListEntry_Given_a_conflict_exception_raised_from_Provider_Returns_Conflict()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();
            string id = mailingListEntry.EmailAddress;
            mockMailingListProvider.Setup( m => m.PostMailingListEntry( mailingListEntry ) ).Throws<ConflictException>();

            // Act
            var result = await unitUnderTest.PostMailingListEntry( mailingListEntry );

            // Assert
            Assert.IsInstanceOf<ConflictResult>( result.Result );
        }

        [Test]
        public async Task PostMailingListEntry_Given_successful_process_Returns_CreatedAtRoute()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            
            var mailingListEntry =_fixture.Build<MailingListEntry>().Create();
            var savedEntry =_fixture.Build<MailingListEntry>().Create();
            mockMailingListProvider.Setup( m => m.PostMailingListEntry( mailingListEntry ) ).Returns( Task.FromResult( savedEntry ) );

            // Act
            var result = await unitUnderTest.PostMailingListEntry( mailingListEntry );
            var contentResult = result.Result as CreatedAtRouteResult;

            // Assert
            Assert.AreSame( savedEntry, contentResult.Value );
            //Assert.True( contentResult.RouteValues.Contains( rv => rv.Key == "id" && rv.Value == mailingListEntry.EmailAddress ) );
        }

        [Test]
        public async Task DeleteMailingListEntry_When_the_entry_does_not_exist_Returns_NotFound()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string id = "asdfds";
            mockMailingListProvider.Setup( m => m.GetMailingListEntry( id ) ).Returns( Task.FromResult( default( MailingListEntry ) ) );

            // Act
            var result = await unitUnderTest.DeleteMailingListEntry( id );

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
        }

        [Test]
        public async Task DeleteMailingListEntry_When_the_entry_exists_Calls_to_delete_and_returns_Ok()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string id = "asdfds";
            
            var entry =_fixture.Build<MailingListEntry>().Create();
            mockMailingListProvider.Setup( m => m.GetMailingListEntry( id ) ).Returns( Task.FromResult( entry ) );
            mockMailingListProvider.Setup( m => m.DeleteMailingListEntry( entry ) ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.DeleteMailingListEntry( id );

            // Assert
            Assert.IsInstanceOf<OkResult>( result.Result );
            mockMailingListProvider.Verify( m => m.DeleteMailingListEntry( entry ), Times.Once() );
            mockMailingListProvider.Verify( m => m.DeleteMailingListEntry( It.IsAny<MailingListEntry>() ), Times.Once() );
        }

        [Test]
        public async Task PostFreeReportRequest_Given_a_mailing_list_entry_is_found_and_FreeReportSent_is_true_The_result_should_be_a_ConflictResponse()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string emailAddress = "asdfsd";
            
            var entry =_fixture.Build<MailingListEntry>().Create();
            entry.FreeReportSent = true;
            mockMailingListProvider.Setup( m => m.GetMailingListEntryByEmail( emailAddress ) ).Returns( Task.FromResult( entry ) );

            // Act
            var result = await unitUnderTest.PostFreeReportRequest(
                emailAddress );

            // Assert
            Assert.IsInstanceOf( typeof( ConflictResult ), result );
        }

        [Test]
        public async Task PostFreeReportRequest_Given_a_mailing_list_entry_is_found_but_the_FreeReportSent_is_False_The_report_should_be_sent()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string emailAddress = "asdfsd";
            
            var entry =_fixture.Build<MailingListEntry>().Create();
            var frResponse =_fixture.Build<PostFreeReportResponse>().Create();
            entry.FreeReportSent = false;
            mockMailingListProvider.Setup( m => m.GetMailingListEntryByEmail( emailAddress ) ).Returns( Task.FromResult( entry ) );
            mockMailingListProvider.Setup( m => m.PostFreeReportRequest( entry ) ).Returns( Task.FromResult( frResponse ) );

            // Act
            var result = await unitUnderTest.PostFreeReportRequest(
                emailAddress );

            // Assert
            Assert.IsInstanceOf( typeof( OkResult ), result );
            mockMailingListProvider.Verify( m => m.PostFreeReportRequest( It.IsAny<String>() ), Times.Never() );
        }

        [Test]
        public async Task PostFreeReportRequest_Given_a_mailing_list_entry_is_not_found_The_report_should_be_sent()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string emailAddress = "asdfsd";
            
            var frResponse =_fixture.Build<PostFreeReportResponse>().Create();
            mockMailingListProvider.Setup( m => m.GetMailingListEntryByEmail( emailAddress ) ).Returns( Task.FromResult( default( MailingListEntry ) ) );
            mockMailingListProvider.Setup( m => m.PostFreeReportRequest( emailAddress ) ).Returns( Task.FromResult( frResponse ) );

            // Act
            var result = await unitUnderTest.PostFreeReportRequest(
                emailAddress );

            // Assert
            Assert.IsInstanceOf( typeof( OkResult ), result );
            mockMailingListProvider.Verify( m => m.PostFreeReportRequest( It.IsAny<MailingListEntry>() ), Times.Never() );
        }

        [Test]
        public async Task PostFreeReportRequest_Given_a_mailing_list_entry_is_not_found_and_the_emailAddress_is_malformed_A_BadRequestResult_should_be_returned()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string emailAddress = "asdfsd";
            
            var frResponse =_fixture.Build<PostFreeReportResponse>().Create();
            mockMailingListProvider.Setup( m => m.GetMailingListEntryByEmail( emailAddress ) ).Returns( Task.FromResult( default( MailingListEntry ) ) );
            mockMailingListProvider.Setup( m => m.PostFreeReportRequest( emailAddress ) ).Throws<FormatException>();

            // Act
            var result = await unitUnderTest.PostFreeReportRequest(
                emailAddress );

            // Assert
            Assert.IsInstanceOf( typeof( BadRequestObjectResult ), result );
            Assert.AreEqual( "Please enter an email address in a valid format!", ((BadRequestObjectResult)result).Value );
            mockMailingListProvider.Verify( m => m.PostFreeReportRequest( It.IsAny<MailingListEntry>() ), Times.Never() );
        }

        [Test]
        public async Task Unsubscribe_Given_deleted_Returns_Ok()
        {
            // Arrange
            var unitUnderTest = CreateMailingListEntriesController();
            string emailAddress = "dasfd";
            mockMailingListProvider.Setup( m => m.Unsubscribe( emailAddress ) ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.Unsubscribe( emailAddress );

            // Assert
            Assert.IsInstanceOf( typeof( OkResult ), result );
            mockMailingListProvider.Verify( m => m.Unsubscribe( emailAddress ), Times.Once() );
            mockMailingListProvider.Verify( m => m.Unsubscribe( It.IsAny<String>() ), Times.Once() );

        }
    }
}
