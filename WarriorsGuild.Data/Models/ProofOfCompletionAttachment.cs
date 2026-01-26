using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.DataAccess.Models
{
    public class ProofOfCompletionAttachment
    {
        public ProofOfCompletionAttachment()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public Guid RequirementId { get; set; }
        public Guid UserId { get; set; }
        [Required]
        public String StorageKey { get; set; }
        [Required]
        public String FileExtension { get; set; }

    }
}