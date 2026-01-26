using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.DataAccess.Models
{
    public class InvitedEmailAddress
    {
        [Key]
        public String EmailAddress { get; set; }

        [Required]
        public Guid InvitedBy { get; set; }

        [Required]
        public DateTime? InvitedAt { get; set; }
    }
}