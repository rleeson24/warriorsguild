using System;
using WarriorsGuild.Data.Models.Ranks.Status;

namespace WarriorsGuild.Rings.Models.Status
{
    public class DeleteRingStatusResponse
    {
        public DeleteRingStatusResponse()
        {
        }
        public bool Success { get; set; }
        public string Error { get; set; }
        public Guid ApprovalRecordId { get; internal set; }
        public RankStatus Status { get; internal set; }
    }
}