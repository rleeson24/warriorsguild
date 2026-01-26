declare var paymentUrls: {
    priceOptionsUrl: string;
    subscriptionsUrl: string;
};
declare var priceOptionStates: string[];
declare var stripePublishableKey;

namespace WarriorsGuild {
    export class SubscriptionsViewModel {
        app: AppViewModel;
        dataModel: {
            SubscriptionsUrl: string;
            PriceOptionsUrl: string;
            PriceOptions: KnockoutObservableArray<WarriorsGuild.Payments.SubscribeablePriceOption>;
            RetrievingPlans: KnockoutObservable<Boolean>;
            SelectedPriceOption: KnockoutObservable<WarriorsGuild.Payments.SubscribeablePriceOption>;
            CurrentSubscription: KnockoutObservable<WarriorsGuild.Payments.ReadOnlySubscription>;
        };
        stripeControl: WarriorsGuild.WgStripe;
        constructor( app: WarriorsGuild.AppViewModel ) {
            this.app = app;
            var self = this;
            this.dataModel = {
                SubscriptionsUrl: paymentUrls.subscriptionsUrl,
                PriceOptionsUrl: paymentUrls.priceOptionsUrl,
                PriceOptions: ko.observableArray<WarriorsGuild.Payments.SubscribeablePriceOption>(),
                RetrievingPlans: ko.observable( false ),
                SelectedPriceOption: ko.observable( null ),
                CurrentSubscription: ko.observable<WarriorsGuild.Payments.ReadOnlySubscription>()
            };
            self.prepareStripeControls(stripePublishableKey);
            app.prepareAjax();

            Sammy( function () {
                this.get( '#paymentOptions', function ( routeParams ) {
                    self.dataModel.RetrievingPlans( true );
                    // Make a call to the protected Web API by passing in a Bearer Authorization Header
                    self.getMySubscription();
                    $.ajax( {
                        method: 'get',
                        url: self.dataModel.PriceOptionsUrl,
                        contentType: "application/json; charset=utf-8",
                        headers: {
                            'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                        },
                        success: function ( data: WarriorsGuild.Payments.SubscribeablePriceOption[] ) {
                            self.dataModel.PriceOptions.removeAll();
                            var subscribedPriceOptionId = '';
                            $.each( data, function ( i, d ) {
                                d["PeriodicPaymentDollar"] = Math.floor( d.charge );
                                d["PeriodicPaymentCents"] = Math.floor( ( d.charge - Math.floor( d.charge ) ) * 100 );
                                d.additionalGuardianPlan["quantity"] = ko.observable<number>( 0 );
                                d.additionalGuardianPlan["formattedCharge"] = koCustom.currencyComputed<number>( d.additionalGuardianPlan.charge );
                                d.additionalGuardianPlan['totalCharge'] = koCustom.currencyComputed<number>( function () {
                                    return ( this.charge * 100 * this.quantity() ) / 100;
                                }, d.additionalGuardianPlan );
                                d.additionalWarriorPlan["quantity"] = ko.observable<number>( 0 );
                                d.additionalWarriorPlan["formattedCharge"] = koCustom.currencyComputed<number>( d.additionalWarriorPlan.charge );
                                d.additionalWarriorPlan['totalCharge'] = koCustom.currencyComputed<number>( function () {
                                    return ( this.charge * 100 * this.quantity() ) / 100;
                                }, d.additionalWarriorPlan );
                                d["formattedCharge"] = koCustom.currencyComputed<number>( d.charge );
                                d["formattedSetupFee"] = koCustom.currencyComputed<number>( d.setupFee );
                                d["subTotal"] = koCustom.currencyComputed<number>( function () {
                                    return self.sumCurrency( this.charge, this.additionalGuardianPlan.totalCharge(), this.additionalWarriorPlan.totalCharge() );
                                },
                                    d );
                                d["totalCharge"] = koCustom.currencyComputed<number>( function () {
                                    return self.sumCurrency( this.setupFee, this.subTotal() );
                                },
                                    d );
                                self.dataModel.PriceOptions.push( d );
                                if ( d.id === routeParams.params.pageLoadPriceOptionId ) {
                                    self.dataModel.SelectedPriceOption( d );
                                }
                            } );
                            self.dataModel.RetrievingPlans( false );
                        },
                        error: function ( err: JQueryXHR ) {
                            BootstrapAlert.alert( {
                                title: "Retrieve Failed!",
                                message: "The Payment Plans could not be retrieved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                            } );
                            self.dataModel.RetrievingPlans( false );
                        },
                    } );
                });
                this.get('/Subscription', function () { this.app.runRoute('get', '#paymentOptions'); });
                this.get('/subscription', function () { this.app.runRoute('get', '#paymentOptions'); });
                this.get( '/Subscription/:pageLoadPriceOptionId', function () {
                    let pageLoadPriceOptionId = this.params['pageLoadPriceOptionId'];
                    this.app.runRoute( 'get', '#paymentOptions', { 'pageLoadPriceOptionId': pageLoadPriceOptionId } );
                } );
            } );
        }

        getMySubscription = (): void => {
            var self = this;
            $.ajax( {
                method: 'get',
                url: self.dataModel.SubscriptionsUrl + '/mine',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function ( data: WarriorsGuild.Payments.Subscription ) {
                    var model = self.createReadOnlySubscription( data );
                    self.dataModel.CurrentSubscription( model );
                },
                error: function ( err: JQueryXHR ) {
                    if ( err.status != 404 ) {
                        BootstrapAlert.alert( {
                            title: "Retrieve Failed!",
                            message: "An error occurred retrieving your subscription" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        } );
                    }
                    else {
                        BootstrapAlert.info( {
                            title: "No Active Subscription!",
                            message: "You have no active subscription.  Subscribe below."
                        } );
                    }
                }
            } );
        }

        //subscribe = ( planId: String ): void => {
        //	var self = this;

        //	$.ajax( {
        //		method: 'post',
        //		url: self.dataModel.SubscriptionsUrl + '/Subscribe?planId=' + planId,
        //		contentType: "application/json; charset=utf-8",
        //		headers: {
        //			'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
        //		},
        //		success: function ( data: string ) {
        //			window.location.assign( data );
        //			//self.dataModel.RetrievingPlans( false );
        //		},
        //		error: function ( err: JQueryXHR ) {
        //			self.actionFailureMessage( 'Subscribe attempt failed' );
        //			if ( err.responseJSON != null ) {
        //				self.actionFailureMessage( self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err) );
        //			}
        //			self.showSaveFailure( true );
        //			self.dataModel.RetrievingPlans( false );
        //		},
        //	} );
        //};

        unsubscribe = (): void => {
            var self = this;
            $.ajax( {
                method: 'post',
                url: self.dataModel.SubscriptionsUrl + '/Unsubscribe',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function ( data: string ) {
                    self.dataModel.CurrentSubscription( null );
                },
                error: function ( err: JQueryXHR ) {
                    BootstrapAlert.alert( {
                        title: "Cancel Subscription Failed!",
                        message: "Subscription cancellation failed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    } );
                    self.dataModel.RetrievingPlans( false );
                },
            } );
        };

        createReadOnlySubscription = ( data: WarriorsGuild.Payments.Subscription ): WarriorsGuild.Payments.ReadOnlySubscription => {
            let model = new WarriorsGuild.Payments.ReadOnlySubscription();
            model.nextPaymentDue = new Date( data.nextPaymentDue );
            model.dateCreated = new Date( data.dateCreated );
            model.lastPaid = new Date( data.lastPaid );
            model.additionalGuardians = data.additionalGuardians;
            model.setupFee = koCustom.currencyComputed<number>( data.setupFee );
            model.charge = koCustom.currencyComputed<number>( data.charge );
            model.additionalCostPerGuardian = koCustom.currencyComputed<number>( data.additionalCostPerGuardian );
            model.totalAdditionalGuardianCost = koCustom.currencyComputed<number>( data.additionalCostPerGuardian * data.additionalGuardians );
            model.additionalCostPerWarrior = koCustom.currencyComputed<number>( data.additionalCostPerWarrior );
            model.totalAdditionalWarriorCost = koCustom.currencyComputed<number>( data.additionalCostPerWarrior * data.additionalWarriors );
            model.totalPeriodicCharge = koCustom.currencyComputed<number>( this.sumCurrency( data.charge, ( data.additionalGuardians * data.additionalCostPerGuardian ), ( data.additionalWarriors * data.additionalCostPerWarrior ) ) );
            model.additionalWarriors = data.additionalWarriors;
            model.paymentMethod = data.paymentMethod;
            model.stripeSubscriptionId = data.stripeSubscriptionId;
            model.priceOptionId = data.priceOptionId;
            model.description = data.description;
            model.frequency = data.frequency;
            model.currency = data.currency;
            model.trialPeriodLength = data.trialPeriodLength;
            model.numberOfGuardians = data.numberOfGuardians;
            model.numberOfWarriors = data.numberOfWarriors;
            model.perks = data.perks;
            model.guardianUsers = $.grep( data.users, function ( item ) {
                return item.isGuardian;
            } );
            model.warriorUsers = $.grep( data.users, function ( item ) {
                return item.isWarrior;
            } );
            return model;
        };

        sumCurrency = ( ...numbers: number[] ): number => {
            var result = 0;
            for ( var i = 0; i < numbers.length; i++ ) {
                result += numbers[i] * 100;
            }
            return result / 100;
        };

        prepareStripeControls = function ( publishableKey: string ) {
            var self = this;
            self.stripeControl = new WarriorsGuild.WgStripe();
            self.stripeControl.prepareStripeControls( publishableKey );
            self.stripeControl.config.tokenHandler = self.stripeTokenHandler;
        }

        stripeTokenHandler = ( token: string ): void => {
            let self = this;
            $.post( {
                url: '/api/Subscription/',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    BasePlanId: self.dataModel.SelectedPriceOption().id,
                    NumberOfAdditionalWarriors: self.dataModel.SelectedPriceOption().additionalWarriorPlan.quantity(),
                    NumberOfAdditionalGuardians: self.dataModel.SelectedPriceOption().additionalGuardianPlan.quantity(),
                    StripePaymentToken: token
                }), headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                }
            } ).then( function ( data: WarriorsGuild.Payments.Subscription ) {
                var model = self.createReadOnlySubscription( data );
                self.dataModel.CurrentSubscription( model );
                BootstrapAlert.success( {
                    title: "Subscription Complete!",
                    message: "Your subscription was successfully created.  Enjoy your Warriors Guild experience!"
                } );
                self.stripeControl.endSubmitSuccess();
            },
                function ( err, status ) {
                    if ( err.status === 404 ) {
                        BootstrapAlert.alert( {
                            title: "Create Subscription Failed!",
                            message: "Your subscription was not successfully created and no charge was placed on your account.  If you continue to have this problem, please notify us using the Contact page." + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        } );
                    }
                    else {
                        BootstrapAlert.alert( {
                            title: "Create Subscription Failed!",
                            message: "Your subscription was not successfully created and no charge was placed on your account.  Please try again" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        } );
                    }
                    self.stripeControl.endSubmitFailure( err.responseJSON != null ? '  ' + err.responseJSON.message : null );
                } );
        }
    }
}

class PriceOptionsDataModel {
    PriceOptionsUrl: string;
    PriceOptions: KnockoutObservableArray<WarriorsGuild.Payments.SubscribeablePriceOption>;
}

WarriorsGuild.app.addViewModel( {
    name: "Subscriptions",
    bindingMemberName: "subscriptions",
    factory: WarriorsGuild.SubscriptionsViewModel
} );



//registerElements = ( elements, exampleName ) => {
//	var self = this;
//	var paymentForm = document.querySelector( '#payment-form' );
//	var form = paymentForm;

//	var resetButton = paymentForm.querySelector( 'a.reset' );
//	var error = form.querySelector( '.error' );
//	var errorMessage = error.querySelector( '.message' );

//	function enableInputs() {
//		Array.prototype.forEach.call(
//			form.querySelectorAll(
//				"input[type='text'], input[type='email'], input[type='tel']"
//			),
//			function ( input ) {
//				input.removeAttribute( 'disabled' );
//			}
//		);
//	}

//	function disableInputs() {
//		Array.prototype.forEach.call(
//			form.querySelectorAll(
//				"input[type='text'], input[type='email'], input[type='tel']"
//			),
//			function ( input ) {
//				input.setAttribute( 'disabled', 'true' );
//			}
//		);
//	}

//	function triggerBrowserValidation() {
//		// The only way to trigger HTML5 form validation UI is to fake a user submit
//		// event.
//		var submit = document.createElement( 'input' );
//		submit.type = 'submit';
//		submit.style.display = 'none';
//		form.appendChild( submit );
//		submit.click();
//		submit.remove();
//	}

//	// Listen for errors from each Element, and show error messages in the UI.
//	var savedErrors = {};
//	elements.forEach( function ( element, idx ) {
//		element.on( 'change', function ( event: Event ) {
//			var stripeEvent = <stripe.elements.ElementChangeResponse><any>event;
//			if ( stripeEvent.error ) {
//				error.classList.add( 'visible' );
//				savedErrors[idx] = stripeEvent.error.message;
//				( <HTMLElement>errorMessage ).innerText = stripeEvent.error.message;
//			} else {
//				savedErrors[idx] = null;

//				// Loop over the saved errors and find the first one, if any.
//				var nextError = Object.keys( savedErrors )
//					.sort()
//					.reduce( function ( maybeFoundError, key ) {
//						return maybeFoundError || savedErrors[key];
//					}, null );

//				if ( nextError ) {
//					// Now that they've fixed the current error, show another one.
//					( <HTMLElement>errorMessage ).innerText = nextError;
//				} else {
//					// The user fixed the last error; no more errors.
//					error.classList.remove( 'visible' );
//				}
//			}
//		} );
//	} );

//	// Listen on the form's 'submit' handler...
//	form.addEventListener( 'submit', function ( e: Event ) {
//		e.preventDefault();
//		self.actionSuccessMessage( '' );
//		self.showSaveSuccess( false );
//		self.actionFailureMessage( '' );
//		self.showSaveFailure( false );
//		var stripe = this.stripe;

//		// Trigger HTML5 validation UI on the form if any of the inputs fail
//		// validation.
//		var plainInputsValid = true;
//		Array.prototype.forEach.call( form.querySelectorAll( 'input' ), function (
//			input
//		) {
//			if ( input.checkValidity && !input.checkValidity() ) {
//				plainInputsValid = false;
//				return;
//			}
//		} );
//		if ( !plainInputsValid ) {
//			triggerBrowserValidation();
//			return;
//		}

//		// Show a loading screen...
//		paymentForm.classList.add( 'submitting' );

//		// Disable all inputs.
//		disableInputs();

//		// Gather additional customer data we may have collected in our form.
//		var name = form.querySelector( '#' + exampleName + '-name' );
//		var address1 = form.querySelector( '#' + exampleName + '-address' );
//		var city = form.querySelector( '#' + exampleName + '-city' );
//		var state = form.querySelector( '#' + exampleName + '-state' );
//		var zip = form.querySelector( '#' + exampleName + '-zip' );
//		var additionalData = {
//			name: name ? ( <HTMLInputElement>name ).value : undefined,
//			address_line1: address1 ? ( <HTMLInputElement>address1 ).value : undefined,
//			address_city: city ? ( <HTMLInputElement>city ).value : undefined,
//			address_state: state ? ( <HTMLInputElement>state ).value : undefined,
//			address_zip: zip ? ( <HTMLInputElement>zip ).value : undefined,
//		};

//		// Use Stripe.js to create a token. We only need to pass in one Element
//		// from the Element group in order to create a token. We can also pass
//		// in the additional customer data we collected in our form.
//		stripe.createToken( elements[0], additionalData ).then( function ( result ) {
//			// Stop loading!
//			paymentForm.classList.remove( 'submitting' );

//			if ( result.token ) {
//				// If we received a token, show the token ID.
//				self.stripeTokenHandler( result.token.id );
//				paymentForm.classList.add( 'submitted' );
//			} else {
//				// Otherwise, un-disable inputs.
//				enableInputs();
//			}
//		} );
//	} );

//	resetButton.addEventListener( 'click', function ( e: Event ) {
//		e.preventDefault();
//		// Resetting the form (instead of setting the value to `''` for each input)
//		// helps us clear webkit autofill styles.
//		( <HTMLFormElement>form ).reset();

//		// Clear each Element.
//		elements.forEach( function ( element ) {
//			element.clear();
//		} );

//		// Reset error state as well.
//		error.classList.remove( 'visible' );

//		// Resetting the form does not un-disable inputs, so we need to do it separately:
//		enableInputs();
//		paymentForm.classList.remove( 'submitted' );
//	} );
//};