using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Models
{
    public class GoalIndexEntry
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Int32 Index { get; set; }
    }
}