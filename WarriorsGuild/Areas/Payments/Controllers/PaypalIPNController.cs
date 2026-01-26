//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.IO;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;

//namespace WarriorsGuild.Areas.Payments.Controllers
//{
//    [ApiController]
//    public class PaypalIPNController : ControllerBase
//    {
//        private class IPNContext
//        {
//            public HttpRequest IPNRequest { get; set; }

//            public string? RequestBody { get; set; }

//            public string? Verification { get; set; } = String.Empty;
//        }

//        [HttpPost]
//        public async Task<ActionResult> Receive()
//        {
//            IPNContext ipnContext = new IPNContext()
//            {
//                IPNRequest = Request
//            };

//            ipnContext.RequestBody = await ipnContext.IPNRequest.Content.ReadAsStringAsync();

//            //Store the IPN received from PayPal
//            LogRequest( ipnContext );

//            //Fire and forget verification task
//            await Task.Run( () => VerifyTask( ipnContext ) );

//            //Reply back a 200 code
//            return Ok();
//        }

//        private void VerifyTask( IPNContext ipnContext )
//        {
//            try
//            {
//                var verificationRequest = WebRequest.Create( "https://www.sandbox.paypal.com/cgi-bin/webscr" );

//                //Set values for the verification request
//                verificationRequest.Method = "POST";
//                verificationRequest.ContentType = "application/x-www-form-urlencoded";

//                //Add cmd=_notify-validate to the payload
//                string? strRequest = "cmd=_notify-validate&" + ipnContext.RequestBody;
//                verificationRequest.ContentLength = strRequest.Length;

//                //Attach payload to the verification request
//                using ( StreamWriter writer = new StreamWriter( verificationRequest.GetRequestStream(), Encoding.ASCII ) )
//                {
//                    writer.Write( strRequest );
//                }

//                //Send the request to PayPal and get the response
//                using ( StreamReader reader = new StreamReader( verificationRequest.GetResponse().GetResponseStream() ) )
//                {
//                    ipnContext.Verification = reader.ReadToEnd();
//                }
//            }
//            catch ( Exception exception )
//            {
//                //Capture exception for manual investigation
//            }

//            ProcessVerificationResponse( ipnContext );
//        }


//        private void LogRequest( IPNContext ipnContext )
//        {
//            // Persist the request values into a database or temporary data store
//        }

//        private void ProcessVerificationResponse( IPNContext ipnContext )
//        {
//            if ( ipnContext.Verification.Equals( "VERIFIED" ) )
//            {
//                try
//                {
//                    var IPNDetails = HttpUtility.ParseQueryString( ipnContext.RequestBody );

//                    var response = new Payments.Models.PaypalIPNDetail();
//                    response.ReceiverEmail = IPNDetails[ "receiver_email" ];
//                    response.ReceiverId = IPNDetails[ "receiver_id" ];
//                    Boolean.TryParse( IPNDetails[ "test_ipn" ], out response.TestIpn );
//                    response.TxnId = IPNDetails[ "txn_id" ];
//                    response.TxnType = IPNDetails[ "txn_type" ];
//                    response.PayerEmail = IPNDetails[ "payer_email" ];
//                    response.PayerStatus = IPNDetails[ "payer_status" ];
//                    Decimal.TryParse( IPNDetails[ "handling_amount" ], out response.HandlingAmount );
//                    response.ItemName = IPNDetails[ "item_name" ];
//                    response.ItemNumber = IPNDetails[ "item_number" ];
//                    response.McCurrency = IPNDetails[ "mc_currency" ];
//                    Decimal.TryParse( IPNDetails[ "mc_fee" ], out response.McFee );
//                    Decimal.TryParse( IPNDetails[ "mc_gross" ], out response.McGross );
//                    DateTime.TryParse( IPNDetails[ "payment_date" ], out response.PaymentDate );
//                    Decimal.TryParse( IPNDetails[ "payment_fee" ], out response.PaymentFee );
//                    Decimal.TryParse( IPNDetails[ "payment_gross" ], out response.PaymentGross );
//                    response.PaymentStatus = IPNDetails[ "payment_status" ];
//                    response.PaymentType = IPNDetails[ "payment_type" ];
//                    response.ProtectionEligibility = IPNDetails[ "protection_eligibility" ];
//                    response.VerifySign = IPNDetails[ "verify_sign" ];

//                    if ( response != null )
//                    {

//                    }
//                }
//                catch ( Exception )
//                {
//                    throw;
//                }
//                // check that Payment_status=Completed
//                // check that Txn_id has not been previously processed
//                // check that Receiver_email is your Primary PayPal email
//                // check that Payment_amount/Payment_currency are correct
//                // process payment
//            }
//            else if ( ipnContext.Verification.Equals( "INVALID" ) )
//            {
//                //Log for manual investigation
//            }
//            else
//            {
//                //Log error
//            }
//        }
//    }
//}