using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Crosses.Crosses.Status;
using WarriorsGuild.Crosses.Mappers;
using WarriorsGuild.Crosses.Models;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Crosses;
using WarriorsGuild.Data.Models.Crosses.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Email;
using WarriorsGuild.Models;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Crosses
{
    public interface ICrossProvider
    {
        Task<CrossViewModel> GetAsync( Guid id, Guid userIdForStatuses );
        Task<IEnumerable<Cross>> ListAsync( Guid userIdForStatuses );
        Task<IEnumerable<CrossQuestionViewModel>> GetQuestionsAsync( Guid crossId, Guid userIdForStatuses );
        Task<CrossViewModel> GetPublicAsync();
        Task<IEnumerable<Cross>> GetCompletedAsync( Guid userIdForStatuses );
        Task<Cross> AddAsync( CreateCrossModel input );
        Task UpdateAsync( Guid crossId, Cross cross );
        //Task SaveQuestionsAsync( Guid id, IEnumerable<CrossQuestion> questions );
        Task SaveAnswersAsync( Guid crossId, Guid userId, IEnumerable<CrossAnswerViewModel> answer );
        Task<Cross> DeleteAsync( Guid crossId );
        Task<IEnumerable<Cross>> UpdateOrderAsync( IEnumerable<GoalIndexEntry> request );
        Task<FileDetail> GetImageAsync( Guid crossId );
        Task UploadImageAsync( Guid crossId, string fileExtension, string localFileName, string mediaType );
        Task<CrossApproval> CompleteAsync( Guid crossId, Guid userId, CrossAnswerViewModel[] answers );
        Task<CrossApproval> CompleteDayAsync( Guid crossId, Guid userId, CrossAnswerViewModel[] answers, Guid dayId );
        Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsAsync( Guid id );
        Task ReturnAsync( Guid crossId, Guid userId, string userReason );
        Task ConfirmCompleteAsync( int crossId, Guid userId );
        Task<IEnumerable<UnassignedCrossViewModel>> GetUnassignedPendingOrApproved( Guid userIdForStatuses );
        Task UploadGuideAsync( Guid crossId, string fileExtension, byte[] file, string mediaType );
        Task<FileDetail> GetGuideAsync( Guid crossId );
        Task<IEnumerable<PinnedCross>> GetActivePinnedCrosses( Guid userIdForStatuses );
        Task PinAsync( Guid crossId, Guid userIdForStatuses );
        Task UnpinAsync( Guid crossId, Guid userIdForStatuses );
        Task SaveAnswersAsync( Guid crossId, Guid dayId, Guid guid, CrossAnswerViewModel[] answers );
        Task<IEnumerable<CrossDayViewModel>> SaveDaysAsync( Guid crossId, IEnumerable<CrossDayViewModel> days );
        Task<IEnumerable<CrossDayViewModel>> GetDaysAsync( Guid crossId, Guid userId );
        Task<IEnumerable<CrossQuestionViewModel>> GetTemplateQuestions( string templateName );
        Task ReturnDay( Guid crossId, Guid dayId, Guid guid, string userReason );
        Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsByCrossAsync( Guid crossId, Guid userId );
        Task<IEnumerable<PinnedCross>> GetPinnedCrosses( Guid userIdForStatuses );
        Task NotifyGuardiansForFiresideChat( Guid crossId, Guid userId );
        Task<IEnumerable<UnassignedCrossViewModel>> GetUnassigned();
    }

    public class CrossProvider : ICrossProvider
    {
        private readonly IEmailProvider emailProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IBlobProvider FileProvider { get; }

        private IGuildDbContext _dbContext { get; }
        private ICrossRepository Repo { get; }
        public ICrossMapper CrossMapper { get; }
        private ILogger<CrossProvider> Logger { get; }

        public CrossProvider( IGuildDbContext dbContext, ICrossRepository repo, ICrossMapper crossMapper, IBlobProvider fileProvider, IEmailProvider emailProvider, IHttpContextAccessor httpContextAccessor, ILogger<CrossProvider> logger )
        {
            _dbContext = dbContext;
            Repo = repo;
            CrossMapper = crossMapper;
            FileProvider = fileProvider;
            this.emailProvider = emailProvider;
            _httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }

        public async Task<IEnumerable<Cross>> ListAsync( Guid userIdForStatuses )
        {
            return await Repo.GetListAsync( userIdForStatuses );
        }

        public async Task<CrossViewModel> GetPublicAsync()
        {
            return CrossMapper.MapToViewModel( await Repo.GetPublicAsync() );
        }

        public async Task<IEnumerable<UnassignedCrossViewModel>> GetUnassigned()
        {
            var crosses = await Repo.Crosses.GroupJoin( _dbContext.RankCrosses, c => c.Id, rc => rc.CrossId, ( c, rc ) => new
            {
                Cross = c,
                R = rc
            } ).SelectMany( c => c.R.DefaultIfEmpty(), ( x, y ) => new { x.Cross, IsUsed = y != null } ).Where( c => !c.IsUsed ).Select( c => c.Cross ).ToArrayAsync();
            return crosses.Select( ra => new UnassignedCrossViewModel()
            {
                Name = ra.Name,
                CrossId = ra.Id,
                ImageUploaded = ra.ImageUploaded,
                ImageExtension = ra.ImageExtension
            } ).ToArray();
        }

        public async Task<IEnumerable<UnassignedCrossViewModel>> GetUnassignedPendingOrApproved( Guid userIdForStatuses )
        {
            var rings = await _dbContext.CrossApprovals.Include( ra => ra.Cross ).Where( ra => ra.UserId == userIdForStatuses && !ra.DayId.HasValue && ra.ApprovedAt.HasValue ).ToArrayAsync();
            return rings.Select( ra => new UnassignedCrossViewModel()
            {
                Name = ra.Cross.Name,
                CrossId = ra.CrossId,
                ImageUploaded = ra.Cross.ImageUploaded,
                ImageExtension = ra.Cross.ImageExtension
            } ).ToArray();
        }

        public async Task<IEnumerable<Cross>> GetCompletedAsync( Guid userIdForStatuses )
        {
            return await Repo.GetCompletedAsync( userIdForStatuses );
        }

        public async Task<CrossViewModel> GetAsync( Guid crossId, Guid userIdForStatuses )
        {
            return await Repo.GetAsync( crossId, userIdForStatuses );
        }

        public async Task<IEnumerable<CrossQuestionViewModel>> GetQuestionsAsync( Guid crossId, Guid userIdForStatuses )
        {
            return await Repo.GetQuestionsAsync( crossId, userIdForStatuses );
        }

        public async Task UpdateAsync( Guid crossId, Cross cross )
        {
            await Repo.UpdateCrossAsync( crossId, cross );
        }

        //public async Task SaveQuestionsAsync( Guid id, IEnumerable<CrossQuestion> questions )
        //{
        //    await Repo.UpdateQuestionsAsync( id, questions );
        //}

        public async Task SaveAnswersAsync( Guid crossId, Guid userId, IEnumerable<CrossAnswerViewModel> answers )
        {
            await Repo.SaveAnswersAsync( crossId, answers.Select( a => new CrossAnswer() { CrossId = crossId, CrossQuestionId = a.CrossQuestionId, Answer = a.Answer, UserId = userId } ) );
        }

        public async Task SaveAnswersAsync( Guid crossId, Guid dayId, Guid userId, CrossAnswerViewModel[] answers )
        {
            await Repo.SaveDayAnswersAsync( crossId, dayId, userId, answers.Select( a => new CrossDayAnswer() { CrossId = crossId, DayId = dayId, QuestionId = a.CrossQuestionId, Answer = a.Answer, UserId = userId } ) );
            await Repo.SaveDayStatusIfNotExists( crossId, dayId, userId );

        }

        public async Task<Cross> AddAsync( CreateCrossModel input )
        {
            var crossCount = await Repo.GetCountAsync();
            var cross = new Cross()
            {
                Id = Guid.NewGuid(),
                Description = input.Description,
                Name = input.Name,
                Index = crossCount
            };
            cross.Questions = new List<CrossQuestion>()
            {
                //new CrossQuestion() { CrossId = cross.Id, Text = $"Identify and describe the background, setting, author, date, and key people for the book of {input.Name}" },
                //new CrossQuestion() { CrossId = cross.Id, Text = $"Write 1-5 sentences summarizing the overall theme of the book of {input.Name}" },
                //new CrossQuestion() { CrossId = cross.Id, Text = $"What is your favorite verse? Commit it to memory and recite for your Guardian during your commendation/ordination." },
                //new CrossQuestion() { CrossId = cross.Id, Text = "In light of what you have just read, {}" },
                //new CrossQuestion() { CrossId = cross.Id, Text = $"List 5 ways on how I can apply what I have learned from the book of {input.Name} in my daily life." }
                //new CrossQuestion() { CrossId = cross.Id, Text = "Pray for focus and wisdom" },
                //new CrossQuestion() { CrossId = cross.Id, Text = "Read {}" },
                //new CrossQuestion() { CrossId = cross.Id, Text = "What jumped out at you while reading the passage?" },
                //new CrossQuestion() { CrossId = cross.Id, Text = "What does this say about God and how are we to respond?" },
                //new CrossQuestion() { CrossId = cross.Id, Text = "What is one thing that I am grateful for today?" },
                //new CrossQuestion() { CrossId = cross.Id, Text = "Who is one person I can pray for and how can I pray for them?" }
            };
            return await Repo.AddAsync( cross );
        }

        public async Task<Cross> DeleteAsync( Guid crossId )
        {
            return await Repo.DeleteAsync( crossId );
        }

        public async Task<IEnumerable<Cross>> UpdateOrderAsync( IEnumerable<GoalIndexEntry> request )
        {
            return await Repo.UpdateOrderAsync( request );
        }

        public async Task UploadImageAsync( Guid crossId, string fileExtension, string f, string mediaType )
        {
            //var uploadedFileResult = await FileProvider.UploadFileAsync( WarriorsGuildFileType.CrossImage, file, crossId.ToString(), mediaType );
            await Repo.SetHasImageAsync( crossId, fileExtension );
        }

        public async Task<FileDetail> GetImageAsync( Guid crossId )
        {
            var fileResult = await FileProvider.DownloadFile( WarriorsGuildFileType.CrossImage, crossId.ToString() );
            return new FileDetail( fileResult.FilePathToServe, null, fileResult.ContentType );
        }

        public async Task UploadGuideAsync( Guid crossId, string fileExtension, byte[] file, string mediaType )
        {
            var uploadedFileResult = await FileProvider.UploadFileAsync( WarriorsGuildFileType.Guide, file, crossId.ToString(), mediaType );
            await Repo.SetHasGuideAsync( crossId, fileExtension );
        }

        public async Task<FileDetail> GetGuideAsync( Guid crossId )
        {
            var fileResult = await FileProvider.DownloadFile( WarriorsGuildFileType.Guide, crossId.ToString() );
            var cross = await _dbContext.Crosses.SingleOrDefaultAsync( r => r.Id == crossId && r.GuideUploaded.HasValue );
            return new FileDetail( fileResult.FilePathToServe, cross?.Name + cross?.GuideFileExtension, fileResult.ContentType );
        }

        public async Task<CrossApproval> CompleteDayAsync( Guid crossId, Guid userId, CrossAnswerViewModel[] answers, Guid dayId )
        {
            CrossApproval result = null;
            var status = (await Repo.GetApprovalRecords( crossId, userId )).SingleOrDefault( s => s.DayId == dayId );
            if ( status == null )
            {
                await SaveAnswersAsync( crossId, dayId, userId, answers );
                await Repo.SaveDayStatusIfNotExists( crossId, dayId, userId );
                if ( (await Repo.GetDay( dayId )).IsCheckpoint )
                {
                    result = await Repo.MarkDayCompleteAsync( crossId, userId, dayId );
                    await NotifyGuardiansForFiresideChat( crossId, userId );
                }
            }
            return result;
        }


        public async Task<CrossApproval> CompleteAsync( Guid crossId, Guid userId, CrossAnswerViewModel[] answers )
        {
            CrossApproval result = null;
            await Repo.SaveAnswersAsync( crossId, answers.Select( a => new CrossAnswer() { CrossId = crossId, CrossQuestionId = a.CrossQuestionId, Answer = a.Answer, UserId = userId } ) );
            var approvalRecords = await Repo.GetApprovalRecords( crossId, userId );
            if ( approvalRecords.All( a => a.ApprovedAt.HasValue ) )
            {
                result = await Repo.MarkCompleteAsync( crossId, userId );
                await NotifyGuardiansForFiresideChat( crossId, userId );
            }
            return result;
        }


        public async Task ReturnAsync( Guid crossId, Guid userId, string userReason )
        {
            var status = await _dbContext.CrossApprovals.Where( c => c.CrossId == crossId && c.DayId == null && c.UserId == userId && !c.ReturnedTs.HasValue && !c.RecalledByWarriorTs.HasValue ).FirstOrDefaultAsync();
            if ( status != null )
            {
                status.ReturnedTs = DateTime.UtcNow;
                status.ReturnedReason = userReason;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ConfirmCompleteAsync( int crossId, Guid userId )
        {
            var status = await _dbContext.CrossApprovals.Where( c => c.Id == crossId && c.UserId == userId && !c.ReturnedTs.HasValue && !c.RecalledByWarriorTs.HasValue ).FirstOrDefaultAsync();
            if ( status != null )
            {
                status.ApprovedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsAsync( Guid id )
        {
            var response = new List<PendingApprovalDetail>();
            var approvalRecords = await _dbContext.CrossApprovals.Include( "Cross" ).Where( ra => ra.UserId == id && !ra.ReturnedTs.HasValue && !ra.RecalledByWarriorTs.HasValue && !ra.ApprovedAt.HasValue ).ToArrayAsync();
            foreach ( var ar in approvalRecords )
            {
                response.Add( new PendingApprovalDetail()
                {
                    ApprovalRecordId = ar.Id,
                    CrossId = ar.CrossId,
                    DayId = ar.DayId,
                    CrossName = ar.Cross.Name,
                    CrossImageUploaded = ar.Cross.ImageUploaded,
                    ImageExtension = ar.Cross.ImageExtension,
                    WarriorCompleted = ar.CompletedAt,
                    GuardianConfirmed = ar.ApprovedAt
                } );
            }
            return response;
        }

        public async Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsByCrossAsync( Guid crossId, Guid userId )
        {
            var response = new List<PendingApprovalDetail>();
            var approvalRecords = await _dbContext.CrossApprovals.Include( "Cross" ).Where( ra => ra.CrossId == crossId && ra.UserId == userId && !ra.ReturnedTs.HasValue && !ra.RecalledByWarriorTs.HasValue && !ra.ApprovedAt.HasValue ).ToArrayAsync();
            foreach ( var ar in approvalRecords )
            {
                response.Add( new PendingApprovalDetail()
                {
                    ApprovalRecordId = ar.Id,
                    CrossId = ar.CrossId,
                    DayId = ar.DayId,
                    CrossName = ar.Cross.Name,
                    CrossImageUploaded = ar.Cross.ImageUploaded,
                    ImageExtension = ar.Cross.ImageExtension,
                    WarriorCompleted = ar.CompletedAt,
                    GuardianConfirmed = ar.ApprovedAt,
                    PercentComplete = ar.PercentComplete
                } );
            }
            return response;
        }

        public async Task<IEnumerable<PinnedCross>> GetActivePinnedCrosses( Guid userIdForStatuses )
        {
            return await Repo.GetActivePinnedCrossesAsync( userIdForStatuses );
        }

        public async Task<IEnumerable<PinnedCross>> GetPinnedCrosses( Guid userIdForStatuses )
        {
            return await Repo.GetPinnedCrosses( userIdForStatuses );
        }

        public async Task PinAsync( Guid id, Guid userIdForStatuses )
        {
            var ring = await Repo.GetAsync( id );
            await Repo.PinAsync( new PinnedCross() { Cross = ring, UserId = userIdForStatuses } );
        }

        public async Task UnpinAsync( Guid crossId, Guid userIdForStatuses )
        {
            var pinnedCross = await Repo.GetPinnedCross( userIdForStatuses, crossId );
            if ( pinnedCross != null )
            {
                await Repo.UnpinAsync( pinnedCross );
            }
        }

        public async Task<IEnumerable<CrossDayViewModel>> SaveDaysAsync( Guid crossId, IEnumerable<CrossDayViewModel> dayVMs )
        {
            return await Repo.SaveDaysAsync( crossId, dayVMs );
        }

        public async Task<IEnumerable<CrossDayViewModel>> GetDaysAsync( Guid crossId, Guid userId )
        {
            var days = await Repo.GetDays( crossId, userId );
            //var statuses = await Repo.GetStatuses( crossId, userId );
            //var result = new List<CrossDayViewModel>();
            //foreach ( var d in days )
            //{
            //    var relatedStatus = statuses.SingleOrDefault( s => s.DayId == d.DayId );
            //    result.Add( new CrossDayViewModel() { Id = d.DayId, Passage = d.Passage, Weight = d.Weight, Index = d.Index, CompletedAt = relatedStatus?.CompletedAt, ApprovedAt = relatedStatus?.ApprovedAt } );
            //}
            return days;
        }

        public async Task<IEnumerable<CrossQuestionViewModel>> GetTemplateQuestions( string templateName )
        {
            return await Repo.GetTemplateQuestions( templateName );
        }

        public async Task ReturnDay( Guid crossId, Guid dayId, Guid userId, string userReason )
        {
            var status = await _dbContext.CrossApprovals.Where( c => c.CrossId == crossId && c.DayId == dayId && c.UserId == userId && !c.ReturnedTs.HasValue && !c.RecalledByWarriorTs.HasValue ).FirstOrDefaultAsync();
            if ( status != null )
            {
                status.ReturnedTs = DateTime.UtcNow;
                status.ReturnedReason = userReason;
                await _dbContext.SaveChangesAsync();
            }
        }

        public Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsByCrossAsync( string userIdForStatuses )
        {
            throw new NotImplementedException();
        }

        public async Task NotifyGuardiansForFiresideChat( Guid crossId, Guid userId )
        {
            try
            {
                var cross = _dbContext.Set<Cross>().Find( crossId );
                var totalCompleted = await Repo.GetTotalCompletedPercent( crossId, userId );

                var guardians = _dbContext.Set<ApplicationUser>().Where( u => u.ChildUsers.Any( cu => cu.Id == userId.ToString() ) ).Select( g => g.Email ).ToArray();
                var user = _dbContext.Set<ApplicationUser>().Find( userId.ToString() );
                var requesting = totalCompleted == 100 ? "Round Table" : "Promotion";
                var httpReq = _httpContextAccessor.HttpContext.Request;
                var htmlBody = $@"<h1>{user.FirstName} {user.LastName.Substring( 0, 1 )} is ready for a fireside chat!</h1><br /><br /><p>He has completed <span style='font-weight: bold'>{totalCompleted}%</span> of <span style='font-weight: bold'>{cross.Name}</span> Cross and is awaiting your confirmation.</p>
                                <a style='color:#af111c' href='{string.Format( "{0}://{1}", httpReq.Scheme, httpReq.Host )}'>Click Here</a> to review.";
                await emailProvider.SendAsync( $"{user.FirstName} {user.LastName.Substring( 0, 1 )} requesting Cross review", htmlBody, guardians, EmailView.Generic );
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, "An error occurred sending request for confirmation", crossId, userId );
            }
        }
    }
}