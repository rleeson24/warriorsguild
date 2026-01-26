using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Areas.Payments.Models.Stripe
{
    public class StripeWebhookMessage
    {
        [Key]
        public String Id { get; set; }
        public string StripeEvent { get; set; }
        public string EventType { get; set; }
        public DateTime Received { get; set; }
        public bool LiveMode { get; set; }
    }
}