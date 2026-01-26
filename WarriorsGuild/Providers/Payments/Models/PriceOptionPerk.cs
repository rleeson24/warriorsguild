using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Providers.Payments.Models
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

        public Int32? Quantity { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public Int32 Index { get; set; }
    }
}