namespace WarriorsGuild.Providers.Payments.Models
{
    public class SaveBillingPlanRequest
    {
        public string? PlanId { get; set; }
        public string? PlanName { get; set; }
        public Decimal SetupFee { get; set; }
        //public Decimal? TrialPlanPrice { get; set; }
        public Int32? TrialPlanLength { get; set; }
        public Frequency PlanFrequency { get; set; }
        public Decimal RegularPlanPrice { get; set; }
        public string? ProductNameForInvoice { get; set; }
        public BillingPlanState State { get; set; }
        public string? Currency { get; set; }
    }
}


/*
		public string? PlanId { get; internal set; }

		public string? RequestUrl { get; internal set; }
		public Int32 MaxFailAttempts { get; internal set; }
		public string? TrialPlanName { get; internal set; }
		public string? RegularPlanName { get; internal set; }
		public string? PlanDescription { get; internal set; }
/*
 * setup_fee only applies to base rate plan definition
 * if( setup_fee > 0 )
 * {
 *  trial_period and trial_cost must be null
 * }
 * else
 * {
 *  max_quantity must be 1
 * }
 * 
 * 
 * 
 * 
 * base plan
 *   -> id
 *   -> setup fee
 *   -> trial period
 *   -> trial price
 *   -> regular price
 *   -> frequency
 *   -> description
 * additional plans
 *   -> id
 *   -> base_plan_id
 *   -> quantity
 *   -> price
 */

