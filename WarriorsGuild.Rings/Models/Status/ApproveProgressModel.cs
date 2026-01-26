using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Rings.Models.Status
{
    public class ApproveProgressModel
    {
        [Required]
        public Guid ApprovalRecordId { get; set; }
        [Required]
        public Guid UserId { get; set; }
    }
}