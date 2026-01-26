namespace WarriorsGuild.Models
{
    public class ProfileViewModel
    {
        public bool ReadOnly { get; internal set; }
        public string? ProfileId { get; }
        public string? GetCurrentAndWorkingRank { get; set; }
        public string? GetPendingApproval { get; }
        public string? MarkAsComplete { get; set; }
        public string? RankImageBase { get; set; }
        public string? FullName { get; internal set; }
        public string? AvatarSrc { get; internal set; }
        public DateTime? PhotoUploaded { get; internal set; }
        public string? RankStatusUrl { get; set; }
        public string? RanksUrl { get; set; }
        public string? RingStatusUrl { get; set; }
        public string? CrossStatusUrl { get; set; }
        public string? CrossUrl { get; private set; }
        public string? ProofOfCompletionUrl { get; }
        public string? FavoriteVerse { get; internal set; }
        public string? Hobbies { get; internal set; }
        public string? InterestingFact { get; internal set; }
        public string? FavoriteMovie { get; internal set; }

        public ProfileViewModel( Boolean readOnly, string? id )
        {
            ReadOnly = readOnly;
            ProfileId = id;
            GetCurrentAndWorkingRank = "/api/Ranks/ByUser/" + id;
            GetPendingApproval = "/api/RankStatus/pendingapproval/" + id;
            MarkAsComplete = "/api/rankstatus/RecordCompletion";
            RankImageBase = "/api/ranks/Image/";
            RanksUrl = "/api/ranks/";
            RankStatusUrl = "/api/rankstatus";
            RingStatusUrl = "/api/ringstatus";
            CrossStatusUrl = "/api/crossStatus";
            CrossUrl = "/api/crosses";
            ProofOfCompletionUrl = "/api/rankstatus/proofOfCompletion";
        }
    }
}