using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Models.Payments
{
    public class BillingViewModel
    {
        [Required]
        public string BasePlanId { get; set; } = default!;
        [Required]
        public int NumberOfAdditionalGuardians { get; set; }
        [Required]
        public int NumberOfAdditionalWarriors { get; set; }
        [Required]
        public string StripePaymentToken { get; set; } = default!;
    }
}