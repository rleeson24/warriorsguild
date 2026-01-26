declare var avatar: string;
declare var avatarContentType: string;
declare var profileEditorUrls: {
	getProfile: string;
	paymentMethods: string;
	avatarUploadUrl: string;
	getAvatarUrl: string;
};

namespace WarriorsGuild {
	export class ProfileEditorViewModel {
		app: AppViewModel;
		SavingProfile: KnockoutObservable<boolean>;
		SaveProfileFailure: KnockoutObservable<boolean>;
		SaveProfileSuccess: KnockoutObservable<boolean>;
		SaveProfileResult: KnockoutComputed<string>;
		stripeControl: WarriorsGuild.WgStripe;
		imgSrcAttr: KnockoutObservable<string>;
		dataModel: {
			Id: KnockoutObservable<string>;
			Email: KnockoutObservable<string>;
			EmailConfirmed: KnockoutObservable<string>;
			FirstName: KnockoutObservable<string>;
			LastName: KnockoutObservable<string>;
			AddressLine1: KnockoutObservable<string>;
			AddressLine2: KnockoutObservable<string>;
			City: KnockoutObservable<string>;
			State: KnockoutObservable<string>;
			PostalCode: KnockoutObservable<string>;
			PhoneNumber: KnockoutObservable<string>;
			ShirtSize: KnockoutObservable<string>;
			UserName: KnockoutObservable<string>;
			PaymentMethods: KnockoutObservableArray<ObservablePaymentMethod>;
			AvatarUploadUrl: string;
			HasAvatar: KnockoutComputed<boolean>;
			Avatar: KnockoutObservable<string>;
			AvatarSrc: KnockoutComputed<string>;
			AvatarContentType: KnockoutObservable<string>;
		};
		constructor( app: WarriorsGuild.AppViewModel, dataModel ) {
			var self = this;
			this.app = app;
			this.SavingProfile = ko.observable<boolean>( false );
			this.SaveProfileFailure = ko.observable<boolean>( false );
			this.SaveProfileSuccess = ko.observable<boolean>( false );
			this.SaveProfileResult = ko.computed( function () {
				return this.SaveProfileSuccess() ? 'Your profile has been successfully updated' :
					this.SaveProfileFailure() ? 'Your profile update failed.  Please try again.' : '';
			}, this );
			this.imgSrcAttr = ko.observable<string>(profileEditorUrls.getAvatarUrl)
			this.dataModel = {
				Id: ko.observable( '' ),
				Email: ko.observable( '' ),
				EmailConfirmed: ko.observable( '' ),
				FirstName: ko.observable( '' ),
				LastName: ko.observable( '' ),
				AddressLine1: ko.observable( '' ),
				AddressLine2: ko.observable( '' ),
				City: ko.observable( '' ),
				State: ko.observable( '' ),
				PostalCode: ko.observable( '' ),
				PhoneNumber: ko.observable( '' ),
				ShirtSize: ko.observable( '' ),
				UserName: ko.observable( '' ),
				PaymentMethods: ko.observableArray<ObservablePaymentMethod>( [] ),
				AvatarUploadUrl: profileEditorUrls.avatarUploadUrl,
				HasAvatar: ko.pureComputed<boolean>( function () { return false; } ),
				AvatarSrc: ko.pureComputed<string>( function () { return ''; } ),
				Avatar: ko.observable<string>( avatar ),
				AvatarContentType: ko.observable<string>(avatarContentType),
			};
			this.dataModel.HasAvatar = ko.pureComputed<boolean>( function () {
				return this.dataModel.Avatar().length > 0;
			}, this );
			this.dataModel.AvatarSrc = ko.pureComputed<string>( function () {
				if ( this.dataModel.HasAvatar() ) {
					return "data:" + this.dataModel.AvatarContentType() + ';base64,' + this.dataModel.Avatar();
				}
				else {
					return '';
				}
			}, this);
			app.prepareAjax();
			self.prepareStripeControls( stripePublishableKey );
			Sammy( function () {
				this.get( '#profileEditor', function () {
					self.SavingProfile( false );
					self.SaveProfileFailure( false );
					self.SaveProfileSuccess( false );
					self.getPaymentMethods();
				});
				this.get('/Manage/EditProfile', function () {
					this.app.runRoute('get', '#profileEditor');
				});
			} );
		}

		SaveProfile = (): void => {
			var self = this;
			self.SavingProfile( true );
			self.SaveProfileFailure( false );
			self.SaveProfileSuccess( false );
			$.ajax( {
				url: profileEditorUrls.getProfile,
				method: 'post',
				data: $( '#form1' ).serialize(),
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function ( data ) {
					self.SavingProfile( false );
					self.SaveProfileSuccess( true );
				},
				error: function ( err: JQueryXHR ) {
					self.SavingProfile( false );
					self.SaveProfileFailure( false );
				}
			} );
		};

		getPaymentMethods = (): void => {
			var self = this;
			$.ajax( {
				url: profileEditorUrls.paymentMethods,
				method: 'get',
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function ( data ) {
					$.each( data.paymentMethods, function ( i, d ) {
						self.dataModel.PaymentMethods.push( self.MapToPaymentMethodObservable( d ) );
					} )
				},
				error: function ( err: JQueryXHR ) {
					BootstrapAlert.alert( {
						title: "Retrieve Failure!",
						message: 'Failed to retrieve your saved payment methods'
					} );
				}
			} );
		};

		setDefaultPaymentMethod = ( data: ObservablePaymentMethod ): void => {
			var self = this;
			$.ajax( {
				url: profileEditorUrls.paymentMethods + '/' + data.id(),
				method: 'put',
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function () {
					$.each( self.dataModel.PaymentMethods(), function ( i, pm ) {
						pm.isDefault( data.id() === pm.id() );
					} );
				},
				error: function ( err: JQueryXHR ) {
					BootstrapAlert.alert( {
						title: "Set Default Failure!",
						message: 'Failed to update your default payment method' + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					} );
				}
			} );
		};

		deletePaymentMethod = ( data: ObservablePaymentMethod ): void => {
			var self = this;
			$.ajax( {
				url: profileEditorUrls.paymentMethods + '/' + data.id(),
				method: 'delete',
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function ( d ) {
					self.dataModel.PaymentMethods.remove( data );
				},
				error: function ( err: JQueryXHR ) {
					BootstrapAlert.alert( {
						title: "Failed to Remove!",
						message: 'Failed to remove the payment method' + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					} );
				}
			} );
		};

		MapToPaymentMethodObservable = ( p: PaymentMethod ): ObservablePaymentMethod => {
			var self = this;
			var mappedPm = ko.mapping.fromJS( p );
			return mappedPm;
		}

		prepareStripeControls = function ( publishableKey: string ) {
			var self = this;
			self.stripeControl = new WarriorsGuild.WgStripe();
			self.stripeControl.prepareStripeControls( publishableKey );
			self.stripeControl.config.tokenHandler = self.stripeTokenHandler;			
		}

		stripeTokenHandler = ( token: string ): void => {
			let self = this;
			$.post( {
				url: '/api/PaymentMethods/?token=' + token,
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				}
			} ).then( function ( data: PaymentMethod ) {
				BootstrapAlert.success( {
					title: "Add Complete!",
					message: "Your payment method was successfully created."
				} );
				var oPM = self.MapToPaymentMethodObservable( data );
				self.dataModel.PaymentMethods.push( oPM );
				self.stripeControl.endSubmitSuccess();
			},
				function ( err, status ) {
					if ( err.status === 404 ) {
						BootstrapAlert.alert( {
							title: "Failed to add payment method!",
							message: "Your payment method was not successfully created.  If you continue to have this problem, please notify us using the Contact page" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
						} );
					}
					else {
						BootstrapAlert.alert( {
							title: "Failed to add payment method!",
							message: "Your payment method was not successfully created.  Please try again" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
						} );
					}
					self.stripeControl.endSubmitFailure( err.responseJSON != null ? '  ' + err.responseJSON.message : null );
				} );
		}

		AddPaymentMethodSuccess = ( data: PaymentMethod ) => {
			var self = this;
			//var model = self.createReadOnlySubscription( data );
			//self.dataModel.CurrentSubscription( model );
			BootstrapAlert.success( {
				title: "Add Complete!",
				message: "Your payment method was successfully added!"
			} );
		};

		handleAddPaymentMethodNotFound = () => {
			BootstrapAlert.alert( {
				title: "Add Failed!",
				message: "Your payment method was not successfully added.  If you continue to have this problem, please notify us using the Contact page."
			} );
		};

		handleAddPaymentMethodError = () => {
			BootstrapAlert.alert( {
				title: "Add Failed!",
				message: "Your payment method was not successfully added.  Please try again."
			} );
		};
	}

	class PaymentMethod {
		id: string;
		brand: string;
		expirationDate: string;
		last4: string;
		isDefault: boolean;
	}

	class ObservablePaymentMethod {
		id: KnockoutObservable<string>;
		brand: KnockoutObservable<string>;
		expirationDate: KnockoutObservable<string>;
		last4: KnockoutObservable<string>;
		isDefault: KnockoutObservable<boolean>;
	}
}

WarriorsGuild.app.addViewModel( {
	name: "ProfileEditor",
	bindingMemberName: "profileEditor",
	factory: WarriorsGuild.ProfileEditorViewModel,
	allowUnauthorized: true
} );

