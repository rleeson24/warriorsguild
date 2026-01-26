using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Payments
{
    public class PriceOptionPerk
    {
        public PriceOptionPerk()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PriceOptionId { get; set; }

        public int? Quantity { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int Index { get; set; }
    }
}