using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models
{
    public class SignedCovenant
    {
        [Required]
        public Guid SignedBy { get; set; }
        [Required]
        public DateTime SignedAt { get; set; }
    }
}
