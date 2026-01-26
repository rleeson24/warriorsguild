using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class SimplePriceOption
    {
        public SimplePriceOption()
        {
        }

        public Guid Id { get; set; }
        public decimal AdditionalGuardianCharge { get; set; }
        public decimal AdditionalWarriorCharge { get; set; }
        public decimal Charge { get; set; }
        public string? Name { get; set; }
        public Frequency Frequency { get; set; }
        public int NumberOfGuardians { get; set; }
        public int NumberOfWarriors { get; set; }
        public List<PriceOptionPerk> Perks { get; set; } = new List<PriceOptionPerk>();
    }
}