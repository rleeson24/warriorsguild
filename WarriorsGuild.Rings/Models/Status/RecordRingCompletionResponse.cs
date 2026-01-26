using System;

namespace WarriorsGuild.Rings.Models.Status
{
    public class RecordRingCompletionResponse
    {
        public RecordRingCompletionResponse()
        {
        }
        public bool Success { get; set; }
        public string Error { get; set; }
        public Guid ApprovalRecordId { get; internal set; }
    }
}