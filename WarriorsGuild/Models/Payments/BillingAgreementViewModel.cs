using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class BillingAgreementViewModel
    {

        public DateTime DateCreated { get; internal set; }
        public DateTime LastPaid { get; set; }
        public DateTime NextPaymentDue { get; set; }
        public DateTime? Cancelled { get; set; }
        public BillingAgreementStatus Status { get; set; }
        public int AdditionalGuardians { get; internal set; }
        public decimal AdditionalCostPerGuardian { get; set; }
        public int AdditionalWarriors { get; internal set; }
        public decimal AdditionalCostPerWarrior { get; set; }
        public PaymentMethod PaymentMethod { get; internal set; }
        public string? StripeSubscriptionId { get; internal set; }


        public Guid PriceOptionId { get; set; }
        public string? Key { get; set; }
        public string? Description { get; set; }
        public Frequency Frequency { get; set; }
        public decimal Charge { get; set; }
        public string? Currency { get; set; }
        public bool Show { get; set; }
        public int? TrialPeriodLength { get; set; }
        public int NumberOfGuardians { get; set; }
        public int NumberOfWarriors { get; set; }
        public decimal SetupFee { get; set; }
        public bool HasTrialPeriod { get; set; }
        public List<PriceOptionPerk> Perks { get; set; } = new List<PriceOptionPerk>();
        public DateTime? Voided { get; set; }
        public List<SubscriptionUser> Users { get; set; } = new List<SubscriptionUser>();
        public bool IsPayingParty { get; internal set; }
    }
}