using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Models.Payments
{
    public class PaypalIPNDetail
    {

        public string? ReceiverEmail { get; internal set; }
        public string? ReceiverId { get; internal set; }

        internal bool TestIpn;
        internal decimal HandlingAmount;
        internal decimal McFee;
        internal decimal McGross;
        internal DateTime PaymentDate;
        internal decimal PaymentFee;
        internal decimal PaymentGross;

        [Key]
        public string? TxnId { get; internal set; }
        public string? TxnType { get; internal set; }
        public string? PayerEmail { get; internal set; }
        public string? PayerStatus { get; internal set; }
        public string? ItemName { get; internal set; }
        public string? ItemNumber { get; internal set; }
        public string? McCurrency { get; internal set; }
        public string? PaymentStatus { get; internal set; }
        public string? PaymentType { get; internal set; }
        public string? ProtectionEligibility { get; internal set; }
        public string? VerifySign { get; internal set; }
    }
}