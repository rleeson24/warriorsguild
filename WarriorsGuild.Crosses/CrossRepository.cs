using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Crosses.Models;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Crosses;
using WarriorsGuild.Data.Models.Crosses.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models;

namespace WarriorsGuild.Crosses
{
    public interface ICrossRepository
    {
        IQueryable<Cross> Crosses { get; }

        Task<IEnumerable<Cross>> GetListAsync( Guid userIdForStatuses );
        Task<Cross> GetAsync( Guid id );
        Task<CrossViewModel> GetAsync( Guid id, Guid userIdForStatuses );
        Task<IEnumerable<CrossQuestionViewModel>> GetQuestionsAsync( Guid id, Guid userIdForStatuses );
        Task<Cross> GetPublicAsync();
        Task<Cross> AddAsync( Cross ring );
        Task UpdateCrossAsync( Guid id, Cross cross );
        Task AddQuestionsAsync( Guid id, IEnumerable<CrossQuestion> questions );
        Task SaveAnswersAsync( Guid id, IEnumerable<CrossAnswer> answers );
        Task<Cross> DeleteAsync( Guid id );
        Task SetHasImageAsync( Guid id, string fileExtension );
        Task SetHasGuideAsync( Guid id, string fileExtension );
        Task<IEnumerable<Cross>> GetCompletedAsync( Guid userIdForStatuses );
        Task<IEnumerable<Cross>> UpdateOrderAsync( IEnumerable<GoalIndexEntry> request );
        Task<int> GetCountAsync();
        Task<PinnedCross> GetPinnedCross( Guid userIdForStatuses, Guid crossId );
        Task<IEnumerable<PinnedCross>> GetActivePinnedCrossesAsync( Guid userIdForStatuses );
        Task PinAsync( PinnedCross cross );
        Task UnpinAsync( PinnedCross cross );

        Task<IEnumerable<CrossApproval>> GetApprovalRecords( Guid id, Guid userId );
        Task<CrossApproval> MarkDayCompleteAsync( Guid id, Guid userid, Guid dayId );
        Task<CrossApproval> MarkCompleteAsync( Guid id, Guid userid );
        Task SaveDayAnswersAsync( Guid crossId, Guid dayId, Guid userId, IEnumerable<CrossDayAnswer> enumerable );
        Task<IEnumerable<CrossDayViewModel>> SaveDaysAsync( Guid crossId, IEnumerable<CrossDayViewModel> days );
        Task<IEnumerable<CrossDayViewModel>> GetDays( Guid crossId, Guid userId );
        Task<IEnumerable<CrossQuestionViewModel>> GetTemplateQuestions( string templateName );
        Task SaveDayStatusIfNotExists( Guid crossId, Guid dayId, Guid userId );
        Task<CrossDay> GetDay( Guid dayId );
        Task<IEnumerable<PinnedCross>> GetPinnedCrosses( Guid userIdForStatuses );
        Task<int> GetTotalCompletedPercent( Guid crossId, Guid userId );

        IQueryable<Cross> GetTemplateCross( Guid crossId );
    }

    public class CrossRepository : ICrossRepository
    {
        private readonly IEnumerable<Tuple<string, string>> TEMPLATE_CROSSES = new Tuple<string, string>[] {
            new Tuple<string, string>("DayQuestions","df1e6a25-b570-4664-831b-2d31e1b03832"),
            new Tuple<string, string>("CrossQuestions","5893b2f4-6ca5-4eed-9ad1-8d1b512574a3")
        };

        private IGuildDbContext _dbContext { get; }

        public CrossRepository( IGuildDbContext db )
        {
            _dbContext = db;
        }

        #region Crosses

        public IQueryable<Cross> Crosses
        {
            get
            {
                return GetQueryable();
            }
        }

        private IQueryable<Cross> GetQueryable()
        {
            //OrderBy must come after DefaultIfEmpty() or Linq does not respect it
            var crosses = _dbContext.Crosses.Where( c => !TEMPLATE_CROSSES.Select( id => id.Item2 ).Contains( c.Id.ToString() ) ).OrderBy( c => c.Index );
            return crosses;
        }

        public IQueryable<Cross> GetTemplateCross( Guid crossId )
        {
            //OrderBy must come after DefaultIfEmpty() or Linq does not respect it
            var crosses = _dbContext.Crosses.Where( c => c.Id == crossId ).OrderBy( c => c.Index );
            return crosses;
        }

        public async Task<IEnumerable<Cross>> GetListAsync( Guid userIdForStatuses )
        {
            //OrderBy must come after DefaultIfEmpty() or Linq does not respect it
            //var crosses = Database.Crosses
            //                                            .GroupJoin( Database.CrossStatusEntries.Where( a => a.UserId == userIdForStatuses ),
            //                                                            c => c.Id.ToString(),
            //                                                            s => s.CrossId.ToString(),
            //                                                            ( q, a ) => new CrossViewModel
            //                                                            {
            //                                                                Id = q.Id,
            //                                                                Description = q.Description,
            //                                                                Name = q.Name,
            //                                                                ImageUploaded = q.ImageUploaded,
            //                                                                Index = q.Index,
            //                                                                Completed = a.DefaultIfEmpty().Any() ? a.DefaultIfEmpty().FirstOrDefault().Completed : (DateTime?)null,
            //                                                                Confirmed = a.DefaultIfEmpty().Any() ? a.DefaultIfEmpty().FirstOrDefault().Confirmed : (DateTime?)null
            //                                                            } );
            //return await crosses.OrderBy( c => c.Index ).ToArrayAsync();
            return await _dbContext.Crosses.Where( c => !TEMPLATE_CROSSES.Select( id => id.Item2 ).Contains( c.Id.ToString() ) ).OrderBy( c => c.Index ).ToArrayAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _dbContext.Crosses.Where( c => !TEMPLATE_CROSSES.Select( id => id.Item2 ).Contains( c.Id.ToString() ) ).OrderBy( rr => rr.Index ).CountAsync();
        }

        public async Task<Cross> GetPublicAsync()
        {
            return await GetQueryable().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Cross>> GetCompletedAsync( Guid userIdForStatuses )
        {
            var completedCrosses = await _dbContext.CrossApprovals.Include( c => c.Cross )
                                                                    .Where( c => c.UserId == userIdForStatuses && c.ApprovedAt.HasValue && !c.DayId.HasValue )
                                                                    .Select( c => c.Cross ).ToArrayAsync();
            return completedCrosses;
        }

        public async Task<CrossViewModel> GetAsync( Guid crossId, Guid userIdForStatuses )
        {
            var result = from c in _dbContext.Crosses.Where( c => c.Id == crossId )
                         join status in _dbContext.CrossApprovals.Where( s => s.DayId == null && s.UserId == userIdForStatuses && !s.ReturnedTs.HasValue && !s.RecalledByWarriorTs.HasValue ) on c.Id equals status.CrossId into status1
                         from s in status1.DefaultIfEmpty()
                         select new { Cross = c, Status = s };
            var cross = await result.SingleOrDefaultAsync();
            return new CrossViewModel
            {
                Id = cross.Cross.Id,
                Description = cross.Cross.Description,
                Name = cross.Cross.Name,
                ExplainText = cross.Cross.ExplainText,
                ImageUploaded = cross.Cross.ImageUploaded,
                ImageExtension = cross.Cross.ImageExtension,
                GuideUploaded = cross.Cross.GuideUploaded,
                GuideExtension = cross.Cross.GuideFileExtension,
                Index = cross.Cross.Index,
                CompletedAt = cross.Status != null ? cross.Status.CompletedAt : (DateTime?)null,
                ApprovedAt = cross.Status != null ? cross.Status.ApprovedAt : null
            };
        }

        public async Task<IEnumerable<CrossQuestionViewModel>> GetQuestionsAsync( Guid crossId, Guid userIdForStatuses )
        {
            IQueryable<CrossQuestionViewModel> questions;
            if ( userIdForStatuses != Guid.Empty )
            {
                var templateCrossId = Guid.Parse( TEMPLATE_CROSSES.First( t => t.Item1 == "CrossQuestions" ).Item2 );
                questions = from q in _dbContext.CrossQuestions.Where( q => q.CrossId == templateCrossId )
                            join a in _dbContext.CrossAnswers.Where( a => a.UserId == userIdForStatuses && a.CrossId == crossId ) on q.Id equals a.CrossQuestionId into a1
                            from a2 in a1.DefaultIfEmpty()
                            select new CrossQuestionViewModel { Id = q.Id, Text = q.Text, Answer = a2 != null ? a2.Answer : string.Empty, Index = q.Index };
            }
            else
            {
                questions = from q in _dbContext.CrossQuestions.Where( q => q.CrossId == Guid.Parse( TEMPLATE_CROSSES.First( t => t.Item1 == "CrossQuestions" ).Item2 ) )
                            select new CrossQuestionViewModel { Id = q.Id, Text = q.Text, Index = q.Index };
            }

            return await questions.OrderBy( rr => rr.Index ).ToArrayAsync();
        }

        public async Task<IEnumerable<CrossQuestionViewModel>> GetDayQuestionsAsync( Guid crossId, Guid dayId, Guid userIdForStatuses )
        {
            IQueryable<CrossQuestionViewModel> questions;
            if ( userIdForStatuses != Guid.Empty )
            {
                questions = from q in _dbContext.CrossQuestions.Where( q => q.CrossId == Guid.Parse( TEMPLATE_CROSSES.First( t => t.Item1 == "DayQuestions" ).Item2 ) )
                            join a in _dbContext.CrossAnswers.Where( a => a.UserId == userIdForStatuses ) on q.Id equals a.CrossQuestionId into a1
                            from a2 in a1.DefaultIfEmpty()
                            select new CrossQuestionViewModel { Id = q.Id, Text = q.Text, Answer = a2 != null ? a2.Answer ?? string.Empty : string.Empty, Index = q.Index };
            }
            else
            {
                questions = from q in _dbContext.CrossQuestions.Where( q => q.CrossId == Guid.Parse( TEMPLATE_CROSSES.First( t => t.Item1 == "DayQuestions" ).Item2 ) )
                            select new CrossQuestionViewModel { Id = q.Id, Text = q.Text, Answer = string.Empty, Index = q.Index };
            }

            return await questions.OrderBy( rr => rr.Index ).ToArrayAsync();
        }

        public async Task<Cross> GetAsync( Guid id )
        {
            //OrderBy must come after DefaultIfEmpty() or Linq does not respect it
            var result = await GetQueryable().SingleOrDefaultAsync( c => c.Id == id );

            return result;
        }

        public async Task UpdateCrossAsync( Guid id, Cross cross )
        {
            var existingCross = await _dbContext.Crosses.FindAsync( id );
            existingCross.Name = cross.Name;
            existingCross.Description = cross.Description;
            existingCross.ExplainText = cross.ExplainText;
            //Database.Entry( existingCross ).CurrentValues.SetValues( cross );

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddQuestionsAsync( Guid id, IEnumerable<CrossQuestion> questions )
        {
            var existingQuestions = await _dbContext.CrossQuestions.Where( q => q.CrossId == id ).ToListAsync();
            // Delete
            foreach ( var existingChild in existingQuestions )
            {
                if ( !questions.Any( c => c.Id == existingChild.Id ) )
                {
                    _dbContext.Entry( existingChild ).State = EntityState.Deleted;
                }
            }

            // Update and Insert
            foreach ( var childModel in questions )
            {
                var existingChild = existingQuestions
                    .Where( c => c.Id == childModel.Id )
                    .SingleOrDefault();

                if ( existingChild != null )
                    // Update
                    _dbContext.Entry( existingChild ).CurrentValues.SetValues( childModel );
                else
                {
                    _dbContext.CrossQuestions.Add( childModel );
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveAnswersAsync( Guid id, IEnumerable<CrossAnswer> answers )
        {
            var existingAnswers = await _dbContext.CrossAnswers.Where( q => q.CrossId == id ).ToListAsync();
            // Delete
            foreach ( var existingChild in existingAnswers )
            {
                if ( !answers.Any( c => c.CrossQuestionId == existingChild.CrossQuestionId ) )
                {
                    _dbContext.Entry( existingChild ).State = EntityState.Deleted;
                }
            }

            // Update and Insert
            foreach ( var childModel in answers )
            {
                var existingChild = existingAnswers
                    .Where( c => c.CrossQuestionId == childModel.CrossQuestionId )
                    .SingleOrDefault();

                if ( existingChild != null )
                    // Update
                    _dbContext.Entry( existingChild ).CurrentValues.SetValues( childModel );
                else
                {
                    _dbContext.CrossAnswers.Add( childModel );
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveDayAnswersAsync( Guid crossId, Guid dayId, Guid userId, IEnumerable<CrossDayAnswer> answers )
        {
            var existingAnswers = await _dbContext.CrossDayAnswers.Where( q => q.CrossId == crossId && q.DayId == dayId && q.UserId == userId ).ToListAsync();
            // Delete
            foreach ( var existingChild in existingAnswers )
            {
                _dbContext.Entry( existingChild ).State = EntityState.Deleted;
            }

            // Update and Insert
            foreach ( var childModel in answers )
            {
                _dbContext.CrossDayAnswers.Add( childModel );
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Cross> AddAsync( Cross cross )
        {
            _dbContext.Crosses.Add( cross );

            await _dbContext.SaveChangesAsync();
            return cross;
        }

        public async Task SetHasImageAsync( Guid crossId, string fileExtension )
        {
            var cross = await _dbContext.Crosses.FindAsync( crossId );
            cross.ImageUploaded = DateTime.UtcNow;
            cross.ImageExtension = fileExtension;
            _dbContext.Entry( cross ).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetHasGuideAsync( Guid crossId, string fileExtension )
        {
            var cross = await _dbContext.Crosses.FindAsync( crossId );
            cross.GuideUploaded = DateTime.UtcNow;
            cross.GuideFileExtension = fileExtension;
            _dbContext.Entry( cross ).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Cross> DeleteAsync( Guid id )
        {
            Cross cross = await _dbContext.Crosses.FindAsync( id );
            if ( cross == null )
            {
                return null;
            }

            _dbContext.Crosses.Remove( cross );
            await _dbContext.SaveChangesAsync();

            return cross;
        }

        public async Task<IEnumerable<Cross>> UpdateOrderAsync( IEnumerable<GoalIndexEntry> request )
        {
            var allCrosses = _dbContext.Crosses.ToArray();
            foreach ( var cross in request )
            {
                var crossToShift = allCrosses.First( r => r.Id == cross.Id );
                _dbContext.Entry( crossToShift ).Entity.Index = cross.Index;
            }
            await _dbContext.SaveChangesAsync();
            return _dbContext.Crosses;
        }

        private bool CrossExists( Guid id )
        {
            return _dbContext.Crosses.Count( e => e.Id == id ) > 0;
        }
        #endregion

        public async Task<IEnumerable<CrossAnswer>> GetAnswersAsync( IEnumerable<Guid> crossIds, string userId )
        {
            return await _dbContext.CrossAnswers.Where( a => crossIds.Contains( a.CrossId ) && a.UserId.ToString() == userId ).ToArrayAsync();
        }

        public async Task<IEnumerable<CrossApproval>> GetApprovalRecords( Guid id, Guid userId )
        {
            return await _dbContext.CrossApprovals.Where( c => c.CrossId == id && c.UserId == userId && !c.RecalledByWarriorTs.HasValue && !c.ReturnedTs.HasValue ).ToArrayAsync();
        }

        public async Task<CrossApproval> MarkDayCompleteAsync( Guid id, Guid userid, Guid dayId )
        {
            var entity = new CrossApproval() { CrossId = id, DayId = dayId, UserId = userid, CompletedAt = DateTime.UtcNow };
            _dbContext.CrossApprovals.Add( entity );
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<CrossApproval> MarkCompleteAsync( Guid id, Guid userid )
        {
            var entity = new CrossApproval() { CrossId = id, UserId = userid, PercentComplete = 100, CompletedAt = DateTime.UtcNow };
            _dbContext.CrossApprovals.Add( entity );
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<PinnedCross> GetPinnedCross( Guid userIdForStatuses, Guid crossId )
        {
            return await _dbContext.PinnedCrosses.Include( p => p.Cross ).FirstOrDefaultAsync( p => p.UserId == userIdForStatuses && p.CrossId == crossId );
        }

        public async Task<IEnumerable<PinnedCross>> GetActivePinnedCrossesAsync( Guid userIdForStatuses )
        {
            var result = new List<PinnedCross>();
            var pinnedCrosses = await (from p in _dbContext.PinnedCrosses.Include( p => p.Cross )
                                       join day in _dbContext.CrossDays on p.CrossId equals day.CrossId into days
                                       from day in days.DefaultIfEmpty()
                                       join dayApproval in _dbContext.CrossDayStatuses.Where( a => a.UserId == userIdForStatuses ) on day.DayId equals dayApproval.DayId into dayApprovals
                                       from dayApproval in dayApprovals.DefaultIfEmpty()
                                       join crossApproval in _dbContext.CrossApprovals.Where( a => a.UserId == userIdForStatuses && !a.DayId.HasValue && !a.RecalledByWarriorTs.HasValue && !a.ReturnedTs.HasValue ) on p.CrossId equals crossApproval.CrossId into approvals
                                       from crossApproval in approvals.DefaultIfEmpty()
                                       where p.UserId == userIdForStatuses
                                       select new { p.Id, Cross = p, Day = day, DayApproval = dayApproval, Approval = crossApproval }
                    ).ToArrayAsync();
            var crosses = pinnedCrosses.Select( p => p.Cross ).Distinct().GroupJoin( pinnedCrosses.Select( p => p.Day ), o => o.CrossId, i => i.CrossId, ( c, days ) => new { PinnedCross = c, Days = days } );
            foreach ( var c in crosses.ToArray() )
            {
                var pinnedCrossList = pinnedCrosses.Where( pc => pc.Cross.CrossId == c.PinnedCross.Cross.Id );
                var approval = pinnedCrossList.FirstOrDefault( pr => pr.Approval != null );
                var approvalCount = pinnedCrossList.Where( pc => pc.DayApproval != null ).Select( pc => pc.DayApproval ).Count() + (approval == null ? 0 : 1);
                c.PinnedCross.PercentComplete = approvalCount * 100 / (c.Days.Count() + 1);
                if ( c.PinnedCross.PercentComplete < 100 || approval == null )
                {
                    result.Add( c.PinnedCross );
                }
            }
            return result;
        }

        public async Task<IEnumerable<PinnedCross>> GetPinnedCrosses( Guid userIdForStatuses )
        {
            var pinnedCrosses = await (from p in _dbContext.PinnedCrosses.Include( p => p.Cross )
                                       where p.UserId == userIdForStatuses
                                       select p
                    ).ToArrayAsync();
            return pinnedCrosses;
        }

        public async Task<IEnumerable<PinnedCross>> GetCompletedPercent( Guid userIdForStatuses )
        {
            var result = new List<PinnedCross>();
            var pinnedCrosses = await (from p in _dbContext.PinnedCrosses.Include( p => p.Cross )
                                       where p.UserId == userIdForStatuses
                                       join day in _dbContext.CrossDays on p.CrossId equals day.CrossId into days
                                       from day in days.DefaultIfEmpty()
                                       join dayApproval in _dbContext.CrossApprovals on p.CrossId equals dayApproval.CrossId into dayApprovals
                                       from dayApproval in dayApprovals.DefaultIfEmpty()
                                       select new { p.Id, Cross = p, Day = day, Approval = dayApprovals.SingleOrDefault( a => a.UserId == userIdForStatuses && a.DayId == day.DayId && !a.RecalledByWarriorTs.HasValue && !a.ReturnedTs.HasValue ) }
                    ).ToArrayAsync();
            var crosses = pinnedCrosses.Select( p => p.Cross ).GroupJoin( pinnedCrosses.Select( p => p.Day ), o => o.CrossId, i => i.CrossId, ( c, days ) => new { Cross = c, Days = days } );
            foreach ( var c in crosses.Select( pc => pc.Cross ).ToArray() )
            {
                var approvals = pinnedCrosses.Where( pc => pc.Cross.CrossId == c.Cross.Id && pc.Approval != null ).Select( pc => pc.Approval ).Count();
                c.PercentComplete = approvals / pinnedCrosses.Select( pc => pc.Day ).Count();
                result.Add( c );
            }
            return result;
        }

        public async Task PinAsync( PinnedCross cross )
        {
            if ( !PinnedCrossExists( cross.UserId, cross.Cross.Id ) )
            {
                _dbContext.PinnedCrosses.Add( cross );
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UnpinAsync( PinnedCross cross )
        {
            _dbContext.PinnedCrosses.Remove( cross );
            await _dbContext.SaveChangesAsync();
        }

        private bool PinnedCrossExists( Guid userId, Guid crossId )
        {
            return _dbContext.PinnedCrosses.Count( e => e.UserId == userId && e.CrossId == crossId ) > 0;
        }

        public async Task<IEnumerable<CrossDayViewModel>> SaveDaysAsync( Guid crossId, IEnumerable<CrossDayViewModel> dayVMs )
        {
            var days = dayVMs.Select( c => new CrossDay()
            {
                CrossId = crossId,
                DayId = c.Id ?? Guid.NewGuid(),
                Passage = c.Passage,
                Weight = c.Weight,
                IsCheckpoint = c.IsCheckpoint,
                Index = c.Index
            } ).ToArray();

            var existingDays = await _dbContext.CrossDays.Where( d => d.CrossId == crossId ).ToListAsync();
            // Delete
            foreach ( var existingChild in existingDays )
            {
                if ( !days.Any( c => c.DayId == existingChild.DayId ) )
                {
                    _dbContext.Entry( existingChild ).State = EntityState.Deleted;
                }
            }

            // Update and Insert
            foreach ( var childModel in days )
            {
                var existingChild = existingDays
                    .Where( c => c.DayId == childModel.DayId )
                    .SingleOrDefault();

                if ( existingChild != null )
                    // Update
                    _dbContext.Entry( existingChild ).CurrentValues.SetValues( childModel );
                else
                {
                    _dbContext.CrossDays.Add( childModel );
                }
            }

            await _dbContext.SaveChangesAsync();
            return days.Select( c => new CrossDayViewModel()
            {
                Id = c.DayId,
                Passage = c.Passage,
                Weight = c.Weight,
                IsCheckpoint = c.IsCheckpoint,
                Index = c.Index
            } ).ToArray();
        }

        public async Task<IEnumerable<CrossDayViewModel>> GetDays( Guid crossId, Guid userId )
        {
            var templateCrossId = TEMPLATE_CROSSES.FirstOrDefault( t => t.Item1.ToLower() == "dayquestions" )?.Item2;
            var query = from day in _dbContext.CrossDays
                        where day.CrossId == crossId
                        join ans in _dbContext.CrossDayAnswers.Where( a => a.UserId == userId ) on day.DayId equals ans.DayId into answers
                        from ans in answers.DefaultIfEmpty()
                        join appr in _dbContext.CrossDayStatuses.Where( a => a.UserId == userId ) on day.DayId equals appr.DayId into approvals
                        from appr in approvals.DefaultIfEmpty()
                        select new { Day = day, Answer = ans, Approval = appr };
            var queryResult = await query.ToArrayAsync();
            var groupByResult = queryResult.GroupBy( r => r.Day, r => r.Answer, ( day, answers ) => new
            {
                Day = day,
                Answers = answers.Where( a => a != null )
            } );
            var approvalDictionary = queryResult.Select( r => new { r.Day, r.Approval } ).Distinct().ToDictionary( r => r.Day, r => r.Approval );
            var qs = await _dbContext.CrossQuestions.Where( q => q.CrossId.ToString() == templateCrossId ).OrderBy( q => q.Index ).ToArrayAsync();
            //return new List<CrossDayViewModel>();
            return groupByResult.Select( c =>
            {
                var approval = approvalDictionary[ c.Day ];
                var dayVM = new CrossDayViewModel()
                {
                    Id = c.Day.DayId,
                    Index = c.Day.Index,
                    Passage = c.Day.Passage,
                    Weight = c.Day.Weight,
                    IsCheckpoint = c.Day.IsCheckpoint,
                    CompletedAt = approval?.CompletedAt
                };
                var questionVms = new List<CrossQuestionViewModel>();
                foreach ( var q in qs )
                {
                    var answer = c.Answers.SingleOrDefault( a => a.QuestionId == q.Id );
                    questionVms.Add( new CrossQuestionViewModel() { Id = q.Id, Index = q.Index, Text = q.Text, Answer = answer?.Answer ?? string.Empty } );
                }
                dayVM.Questions = questionVms;
                return dayVM;
            } ).OrderBy( d => d.Index ).ToArray();
        }

        public async Task<IEnumerable<CrossQuestionViewModel>> GetTemplateQuestions( string templateName )
        {
            var id = TEMPLATE_CROSSES.FirstOrDefault( t => t.Item1.ToLower() == templateName.ToLower() )?.Item2;
            var questions = await _dbContext.CrossQuestions.Where( q => q.CrossId.ToString() == id ).OrderBy( q => q.Index ).ToArrayAsync();
            return questions.Select( q => new CrossQuestionViewModel { Id = q.Id, Text = q.Text, Index = q.Index } );
        }

        public async Task SaveDayStatusIfNotExists( Guid crossId, Guid dayId, Guid userId )
        {
            var dayStatus = await _dbContext.CrossDayStatuses.Where( s => s.DayId == dayId && s.UserId == userId ).SingleOrDefaultAsync();
            if ( dayStatus == null )
            {
                _dbContext.CrossDayStatuses.Add( new CrossDayStatus() { UserId = userId, DayId = dayId, CompletedAt = DateTime.UtcNow } );
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<CrossDay> GetDay( Guid dayId )
        {
            return await _dbContext.CrossDays.Where( d => d.DayId == dayId ).SingleOrDefaultAsync();
        }

        public async Task<int> GetTotalCompletedPercent( Guid crossId, Guid userId )
        {
            var result = 0;
            var pinnedCrossList = await (from p in _dbContext.PinnedCrosses.Include( p => p.Cross ).Where( c => c.CrossId == crossId )
                                         join day in _dbContext.CrossDays on p.CrossId equals day.CrossId into days
                                         from day in days.DefaultIfEmpty()
                                         join dayApproval in _dbContext.CrossDayStatuses.Where( a => a.UserId == userId ) on day.DayId equals dayApproval.DayId into dayApprovals
                                         from dayApproval in dayApprovals.DefaultIfEmpty()
                                         join crossApproval in _dbContext.CrossApprovals.Where( a => a.UserId == userId && !a.DayId.HasValue && !a.RecalledByWarriorTs.HasValue && !a.ReturnedTs.HasValue ) on p.CrossId equals crossApproval.CrossId into approvals
                                         from crossApproval in approvals.DefaultIfEmpty()
                                         where p.UserId == userId
                                         select new { p.Id, Cross = p, Day = day, DayApproval = dayApproval, Approval = crossApproval }
                    ).ToArrayAsync();
            var approval = pinnedCrossList.FirstOrDefault( pr => pr.Approval != null );
            var approvalCount = pinnedCrossList.Where( pc => pc.DayApproval != null ).Select( pc => pc.DayApproval ).Count() + (approval == null ? 0 : 1);
            result = approvalCount * 100 / (pinnedCrossList.Count() + 1);
            return result;
        }
    }
}