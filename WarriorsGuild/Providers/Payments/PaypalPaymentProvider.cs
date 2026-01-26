//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using WarriorsGuild.Providers.Payments.Models;
//using static WarriorsGuild.Providers.EmailProvider;

//namespace WarriorsGuild.Providers.Payments
//{
//    public class PaypalPaymentProvider
//    {
//        private string _accessToken;
//        private APIContext _apiContext;

//        private IEmailProvider _emailProvider;
//        private IEmailProvider EmailProvider
//        {
//            get
//            {
//                return _emailProvider ?? (_emailProvider = new EmailProvider());
//            }
//        }

//        private string AccessToken
//        {
//            get
//            {
//                if ( _accessToken == null )
//                {
//                    // Get a reference to the config
//                    var config = ConfigManager.Instance.GetProperties();

//                    // Use OAuthTokenCredential to request an access token from PayPal
//                    _accessToken = new OAuthTokenCredential( config ).GetAccessToken();
//                }
//                return _accessToken;
//            }
//        }

//        private APIContext ApiContext
//        {
//            get
//            {
//                if ( _apiContext == null )
//                {
//                    _apiContext = new APIContext( AccessToken );
//                    // Initialize the apiContext's configuration with the default configuration for this application.
//                    _apiContext.Config = ConfigManager.Instance.GetProperties();

//                    // Define any custom configuration settings for calls that will use this object.
//                    _apiContext.Config[ "connectionTimeout" ] = "6000"; // Quick timeout for testing purposes

//                    // Define any HTTP headers to be used in HTTP requests made with this APIContext object
//                    if ( _apiContext.HTTPHeaders == null )
//                    {
//                        _apiContext.HTTPHeaders = new Dictionary<string, string>();
//                    }
//                    _apiContext.HTTPHeaders[ "some-header-name" ] = "some-value";
//                }
//                return _apiContext;
//            }
//        }

//        public IEnumerable<Models.Plan> GetBillingPlanList()
//        {
//            try
//            {
//                var createdPlansTask = Task.Run<IEnumerable<PayPal.Api.Plan>>( () =>
//                {
//                    var response = PayPal.Api.Plan.List( ApiContext, "0", "CREATED" );
//                    if ( response != null )
//                    {
//                        return response.plans;
//                    }
//                    else
//                    {
//                        return new PayPal.Api.Plan[ 0 ];
//                    }
//                } );
//                var activePlansTask = Task.Run<IEnumerable<PayPal.Api.Plan>>( () =>
//                {
//                    var response = PayPal.Api.Plan.List( ApiContext, "0", "ACTIVE" );
//                    if ( response != null )
//                    {
//                        return response.plans;
//                    }
//                    else
//                    {
//                        return new PayPal.Api.Plan[ 0 ];
//                    }
//                } );
//                var inactivePlansTask = Task.Run<IEnumerable<PayPal.Api.Plan>>( () =>
//                {
//                    var response = PayPal.Api.Plan.List( ApiContext, "0", "INACTIVE" );
//                    if ( response != null )
//                    {
//                        return response.plans;
//                    }
//                    else
//                    {
//                        return new PayPal.Api.Plan[ 0 ];
//                    }
//                } );
//                try
//                {
//                    Task.WaitAll( createdPlansTask, activePlansTask, inactivePlansTask );
//                }
//                catch ( AggregateException ae )
//                {
//                    throw ae.Flatten();
//                }
//                return createdPlansTask.Result.Concat( activePlansTask.Result ).Concat( inactivePlansTask.Result ).Select( r => new Models.Plan() { Id = r.id, Status = r.state } );
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( String.Empty, "GetBillingPlanList", ex );
//                return null;
//            }
//        }

//        public Models.Plan GetBillingPlan( string planId )
//        {
//            try
//            {
//                var plan = PayPal.Api.Plan.Get( ApiContext, planId );
//                return new Models.Plan()
//                {
//                    Id = plan.id,
//                    Status = plan.state
//                };
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( planId, "GetBillingPlan", ex );
//                return null;
//            }
//        }

//        public String CreateBillingPlan( SaveBillingPlanRequest request )
//        {
//            try
//            {
//                var plan = CreatePlanObject( request );
//                // Call `plan.Create()` to create the billing plan resource.
//                var createdPlan = plan.Create( ApiContext );

//                return createdPlan.id;
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( request, "CreateBillingPlan", ex );
//                return null;
//            }
//        }

//        private static PayPal.Api.Plan CreatePlanObject( SaveBillingPlanRequest request )
//        {
//            // ### Create the Billing Plan
//            // Define the plan and attach the payment definitions and merchant preferences.
//            // More Information: https://developer.paypal.com/webapps/developer/docs/api/#create-a-plan
//            var response = new PayPal.Api.Plan
//            {
//                name = request.PlanName,
//                //description = request.PlanDescription,
//                type = "infinite",
//                // Define the merchant preferences.
//                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#merchantpreferences-object
//                merchant_preferences = new MerchantPreferences()
//                {
//                    setup_fee = GetCurrency( request.SetupFee.ToString() ),
//                    //return_url = request.RequestUrl,
//                    //cancel_url = request.RequestUrl.ToString() + "?cancel=true",
//                    auto_bill_amount = "YES",
//                    initial_fail_amount_action = "CONTINUE",
//                    //max_fail_attempts = request.MaxFailAttempts.ToString()
//                },
//                payment_definitions = new List<PaymentDefinition>
//                {
//                }
//            };

//            if ( request.TrialPlanLength.HasValue )
//            {
//                // Define a trial plan that will only charge $9.99 for the first
//                // month. After that, the standard plan will take over for the
//                // remaining 11 months of the year.
//                response.payment_definitions.Add( new PaymentDefinition()
//                {
//                    name = request.PlanName,
//                    type = "TRIAL",
//                    frequency = MapFrequency( request.PlanFrequency ),
//                    frequency_interval = "1",
//                    //amount = GetCurrency( request.TrialPlanPrice.ToString() ),
//                    cycles = request.TrialPlanLength.ToString(),
//                    charge_models = new List<ChargeModel>()
//                } );
//            }
//            // Define the standard payment plan. It will represent a monthly
//            // plan for $19.99 USD that charges once month for 11 months.
//            response.payment_definitions.Add( new PaymentDefinition
//            {
//                name = request.PlanName,
//                type = "REGULAR",
//                frequency = MapFrequency( request.PlanFrequency ),
//                frequency_interval = "1",
//                amount = GetCurrency( request.RegularPlanPrice.ToString() ),
//                // > NOTE: For `IFNINITE` type plans, `cycles` should be 0 for a `REGULAR` `PaymentDefinition` object.
//                cycles = "0",
//                charge_models = new List<ChargeModel>()
//            } );
//            return response;
//        }

//        private static PayPal.Api.Plan CreateUpdatePlanObject( SaveBillingPlanRequest request )
//        {
//            // ### Create the Billing Plan
//            // Define the plan and attach the payment definitions and merchant preferences.
//            // More Information: https://developer.paypal.com/webapps/developer/docs/api/#create-a-plan
//            return new PayPal.Api.Plan
//            {
//                name = request.PlanName,
//                //description = request.PlanDescription,
//                type = "infinite"
//            };
//        }

//        public Boolean UpdateBillingPlan( SaveBillingPlanRequest request )
//        {
//            try
//            {

//                // In order to update the plan, you must define one or more
//                // patches to be applied to the plan. The patches will be
//                // applied in the order in which they're specified.
//                //
//                // The 'value' of each Patch object will need to be a Plan object
//                // that contains the fields that will be modified.
//                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#patchrequest-object
//                var tempPlan = CreateUpdatePlanObject( request );

//                // NOTE: Only the 'replace' operation is supported when updating
//                //       billing plans.
//                var patchRequest = new PatchRequest()
//            {
//                new Patch()
//                {
//                    op = "replace",
//                    path = "/",
//                    value = tempPlan
//                }
//            };

//                // Get the plan we want to update.
//                var planId = request.PlanId;

//                var plan = PayPal.Api.Plan.Get( ApiContext, planId );

//                // Update the plan.
//                plan.Update( ApiContext, patchRequest );
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( request, "GetPayment", ex );
//                return false;
//            }
//            return true;
//        }

//        public Boolean ActivateBillingPlan( string planId )
//        {
//            try
//            {
//                ChangePlanState( planId, BillingPlanState.Active );
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( planId, "ActivateBillingPlan", ex );
//                return false;
//            }
//            return true;
//        }

//        public Boolean DeactivateBillingPlan( string planId )
//        {
//            try
//            {
//                ChangePlanState( planId, BillingPlanState.Deleted );
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( planId, "DeactivateBillingPlan", ex );
//                return false;
//            }
//            return true;
//        }

//        public Boolean DeleteBillingPlan( string planId )
//        {
//            try
//            {
//                ChangePlanState( planId, BillingPlanState.Deleted );
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( planId, "DeleteBillingPlan", ex );
//                return false;
//            }
//            return true;
//        }

//        private void ChangePlanState( string planId, BillingPlanState state )
//        {
//            PayPal.Api.Plan plan = null;
//            // Retrieve Plan
//            try
//            {
//                plan = PayPal.Api.Plan.Get( ApiContext, planId );

//                // Activate the plan
//                var patchRequest = new PatchRequest()
//                {
//                    new Patch()
//                    {
//                        op = "replace",
//                        path = "/",
//                        value = new PayPal.Api.Plan { state = state.ToString().ToUpper() }
//                    }
//                };
//                plan.Update( ApiContext, patchRequest );
//            }
//            catch ( PaymentsException ex )
//            {
//                if ( ex.StatusCode == System.Net.HttpStatusCode.BadRequest )
//                {
//                    throw;
//                }
//                else if ( ex.StatusCode == System.Net.HttpStatusCode.InternalServerError )
//                {
//                    throw;
//                }
//            }
//        }

//        private static string MapFrequency( Frequency frequency )
//        {
//            return frequency == Frequency.Monthly ? "MONTH" : "YEAR";
//        }

//        private static Currency GetCurrency( string value )
//        {
//            return new Currency() { value = value, currency = "USD" };
//        }

//        internal CreatedAgreement CreateBillingAgreement( string planId, WarriorsGuild.Providers.Payments.Models.Address shippingAddress, string name, string description, DateTime startDate )
//        {
//            try
//            {
//                var agreement = new Agreement()
//                {
//                    name = name,
//                    description = description,
//                    start_date = startDate.ToString( "yyyy-MM-ddTHH:mm:ss" ) + "Z",
//                    payer = new Payer() { payment_method = "paypal" },
//                    plan = new PayPal.Api.Plan() { id = planId }
//                };


//                var createdAgreement = agreement.Create( ApiContext );
//                return new CreatedAgreement() { Token = createdAgreement.token, ApprovalUrl = createdAgreement.GetApprovalUrl() };
//            }
//            catch ( PaymentsException ex )
//            {
//                HandleException( new { PlanId = planId, ShippingAddress = shippingAddress, Name = name, Description = description, StartDate = startDate }, "DeleteBillingPlan", ex );
//                return null;
//            }
//        }

//        public void ExecuteBillingAgreement( string token )
//        {
//            var agreement = new Agreement() { token = token };
//            var executedAgreement = agreement.Execute( ApiContext );
//        }

//        public void SuspendBillingAgreement( string agreementId )
//        {
//            var agreement = new Agreement() { id = agreementId };
//            agreement.Suspend( ApiContext, new AgreementStateDescriptor()
//            { note = "Suspending the agreement" } );
//        }

//        public void ReactivateBillingAgreement( string agreementId )
//        {
//            var agreement = new Agreement() { id = agreementId };
//            agreement.ReActivate( ApiContext, new AgreementStateDescriptor()
//            { note = "Reactivating the agreement" } );
//        }

//        public void CancelBillingAgreement( string agreementId )
//        {
//            var agreement = new Agreement() { id = agreementId };
//            agreement.Cancel( ApiContext, new AgreementStateDescriptor()
//            { note = "Cancelling the agreement" } );
//        }


//        private void HandleException( object debugData, string method, PaymentsException ex )
//        {
//            var swException = new StringWriter();
//            var swDebugData = new StringWriter();
//            var serializer = Newtonsoft.Json.JsonSerializer.Create();
//            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
//            serializer.Serialize( swDebugData, debugData );
//            serializer.Serialize( swException, ex );
//            var message = new EmailProvider.EmailMessage();
//            message.Recipients = "rleeson_2000@yahoo.com";
//            message.Subject = "Exception Occurred in WarriorsGuild";
//            message.TextBody = String.Format( "Payment error occurred in {0} for: \r\n {1} \r\n {2}",
//                                        method,
//                                        debugData,
//                                        swException.ToString() );
//            message.HtmlBody = EmailProvider.RenderEmailViewToString( EmailView.Generic, message.TextBody );

//            Task.Run( async () => await EmailProvider.SendAsync( message ) ).Wait();
//        }
//    }
//}