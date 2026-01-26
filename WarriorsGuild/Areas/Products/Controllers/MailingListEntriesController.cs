using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Areas.Products.Controllers
{
    [ApiController]
    [Route( "api/MailingList" )]
    [IgnoreAntiforgeryToken]
    public class MailingListEntriesController : ControllerBase
    {
        private IMailingListProvider _mailingListProvider;
        private IMailingListProvider Provider
        { get { return _mailingListProvider; } }

        public MailingListEntriesController( IMailingListProvider mailingListProvider )
        {
            _mailingListProvider = mailingListProvider;
        }

        // GET: api/MailingListEntries
        [HttpGet]
        public IQueryable<MailingListEntry> GetMailingList()
        {
            return Provider.GetMailingList();
        }

        // GET: api/MailingListEntries/5
        [HttpGet( "{id}" )]
        public async Task<ActionResult<MailingListEntry>> GetMailingListEntry( string id )
        {
            var mailingListEntry = await Provider.GetMailingListEntry( id );
            if ( mailingListEntry == null )
            {
                return NotFound();
            }

            return Ok( mailingListEntry );
        }

        // PUT: api/MailingListEntries/5
        [HttpPut( "{id}" )]
        public async Task<ActionResult> PutMailingListEntry( string id, MailingListEntry mailingListEntry )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            if ( id != mailingListEntry.EmailAddress )
            {
                return BadRequest();
            }

            MailingListEntry result;
            try
            {
                result = await Provider.PutMailingListEntry( id, mailingListEntry );
            }
            catch ( ConflictException )
            {
                return Conflict();
            }

            return NoContent();
        }

        // POST: api/MailingListEntries
        [HttpPost]
        public async Task<ActionResult<MailingListEntry>> PostMailingListEntry( MailingListEntry mailingListEntry )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            MailingListEntry result;
            try
            {
                result = await Provider.PostMailingListEntry( mailingListEntry );
            }
            catch ( ConflictException )
            {
                return Conflict();
            }

            return CreatedAtRoute( "DefaultApi", new { id = result.EmailAddress }, result );
        }

        // DELETE: api/MailingListEntries/5
        [HttpDelete]
        public async Task<ActionResult<MailingListEntry>> DeleteMailingListEntry( string id )
        {
            var entry = await Provider.GetMailingListEntry( id );
            if ( entry == null )
            {
                return NotFound();
            }

            await Provider.DeleteMailingListEntry( entry );
            return Ok();
        }


        [HttpPost( "RequestFreeReport" )]
        public async Task<ActionResult> PostFreeReportRequest( [FromBody] string emailAddress )
        {
            var entry = await Provider.GetMailingListEntryByEmail( emailAddress.ToLower() );
            if ( entry == null )
            {
                try
                {
                    await Provider.PostFreeReportRequest( emailAddress );
                }
                catch ( FormatException )
                {
                    return BadRequest( "Please enter an email address in a valid format!" );
                }

                return Ok();
            }
            else if ( !entry.FreeReportSent )
            {
                await Provider.PostFreeReportRequest( entry );
                return Ok();
            }
            return Conflict();
        }


        [HttpPost( "Unsubscribe" )]
        public async Task<ActionResult> Unsubscribe( string emailAddress )
        {
            await Provider.Unsubscribe( emailAddress.ToLower() );
            return Ok();
        }
    }
}