namespace WarriorsGuild.Providers.Payments.Models
{
    internal class Address
    {
        public string? line1 { get; set; }
        public string? line2 { get; set; }
        public string? city { get; set; }
        public string? country_code { get; set; }
        public string? postal_code { get; set; }
        public string? state { get; set; }
        public string? id { get; set; }
        public string? recipient_name { get; set; }
        public bool? default_address { get; set; }
        public bool? preferred_address { get; set; }
        public string? phone { get; set; }
        public string? normalization_status { get; set; }
        public string? status { get; set; }
        public string? type { get; set; }

    }
}