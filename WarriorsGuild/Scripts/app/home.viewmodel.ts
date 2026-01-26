declare var homeUrls: {
	priceOptionsUrl: string;
};

namespace WarriorsGuild {
	export class HomeViewModel {
		dataModel: {
			EmailAddress: KnockoutObservable<string>;
			BlogEntries: KnockoutObservableArray<object>;
			PriceOptionsUrl: string;
			PriceOptions: KnockoutObservableArray<DisplayablePriceOption>;
			RetrievingPlans: KnockoutObservable<boolean>;
		};
		freeReportButtonEnabled: KnockoutObservable<boolean>;
		freeReportSuccessfreeReportError: KnockoutObservable<boolean>;
		freeReportWarning: KnockoutObservable<boolean>;
		freeReportError: KnockoutObservable<boolean>;
		freeReportSuccess: KnockoutObservable<boolean>;
		freeReportErrorMessage: KnockoutObservable<string>;
		retrievePlansFailure: KnockoutObservable<boolean>;
		constructor( app: WarriorsGuild.AppViewModel, dataModel ) {
			var self = this;
			this.dataModel = {
				EmailAddress: ko.observable( "" ),
				BlogEntries: ko.observableArray( [] ),
				PriceOptionsUrl: homeUrls.priceOptionsUrl,
				PriceOptions: ko.observableArray<DisplayablePriceOption>(),
				RetrievingPlans: ko.observable<boolean>( false )
			};
			self.freeReportButtonEnabled = ko.observable( true );
			self.freeReportWarning = ko.observable( false );
			self.freeReportSuccess = ko.observable( false );
			self.freeReportError = ko.observable( false );
			self.freeReportErrorMessage = ko.observable( '' );
			self.retrievePlansFailure = ko.observable( false );
			this.dataModel.EmailAddress = ko.observable( '' );
			this.dataModel.BlogEntries = ko.observableArray([]);
			app.prepareAjax(false);
			Sammy( function () {
				this.get( '#home', function () {
					// Make a call to get blog entries
					//$.ajax( {
					//	method: 'get',
					//	url: 'https://public-api.wordpress.com/rest/v1.1/sites/www.warriorsguild.com/posts/?number=2',
					//	contentType: "application/json; charset=utf-8",
					//	success: function ( data ) {
					//		$.each( data.posts, function ( index, data ) {
					//			self.dataModel.BlogEntries.push( data );
					//		} );
					//	}
					//} );
					self.dataModel.RetrievingPlans( true );
					self.retrievePlansFailure( false );
					$.ajax( {
						method: 'get',
						url: self.dataModel.PriceOptionsUrl + '/simple',
						contentType: "application/json; charset=utf-8",
						//headers: {
						//	'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
						//},
						success: function ( data: SimplePriceOption[] ) {
							self.dataModel.PriceOptions.removeAll();
							$.each( data, function ( i, d ) {
								let po = self.CreateDisplayablePriceOption( d );
								self.dataModel.PriceOptions.push( po );
							} );
							self.dataModel.RetrievingPlans( false );
						},
						error: function ( err: JQueryXHR ) {
							self.retrievePlansFailure( true );
							self.dataModel.RetrievingPlans( false );
						},
					} );
				} );
				this.get( '/', function () { this.app.runRoute( 'get', '#home' ); } );
			} );
		}

		CreateDisplayablePriceOption = ( data: SimplePriceOption ): DisplayablePriceOption => {
			let result = new DisplayablePriceOption();
			result.Id = data.id;
			result.Perks = data.perks;
			result.Description = data.description;
			result.NumberOfGuardians = data.numberOfGuardians;
			result.NumberOfWarriors = data.numberOfWarriors;
			result.Charge = koCustom.currencyComputed<number>( data.charge );
			result.Name = data.name;
			result.AdditionalGuardianCharge = koCustom.currencyComputed<number>( data.additionalGuardianCharge );
			result.AdditionalWarriorCharge = koCustom.currencyComputed<number>( data.additionalWarriorCharge );
			result.Frequency = data.frequency;
			result.IncludedGuardiansAndWarriorsString = '(Includes ' + result.NumberOfGuardians + ' Guardian' + ( result.NumberOfGuardians > 1 ? 's' : '' ) + ' & '
				+ result.NumberOfWarriors + ' Warrior' + ( result.NumberOfWarriors > 1 ? 's' : '' )
				+ ' login' + ( (result.NumberOfGuardians + result.NumberOfWarriors) > 2 ? 's' : '' ) + ')';
			return result;
		}

		RequestFreeReport = (): void =>  {
			var self = this;
			if ( self.dataModel.EmailAddress() !== null && self.dataModel.EmailAddress().trim() > '' ) {
				self.freeReportButtonEnabled( false );
				self.freeReportError( false );
				self.freeReportSuccess( false );
				self.freeReportWarning( false );
				// Make a call to the protected Web API by passing in a Bearer Authorization Header
				$.ajax( {
					method: 'post',//need bearer token??
					url: 'api/MailingList/RequestFreeReport',
					contentType: "application/json; charset=utf-8",
					data: '"' + self.dataModel.EmailAddress() + '"',
					success: function ( data ) {
						self.dataModel.EmailAddress( '' );
						self.freeReportButtonEnabled( true );
						self.freeReportSuccess( true );
					},
					error: function ( err: JQueryXHR ) {
						self.freeReportButtonEnabled( true );
						self.freeReportError( true );
						if ( err.status === 409 ) {
							self.freeReportErrorMessage( "This free report has already been sent to this email address." );
						}
						else {
							var responseMessage = WarriorsGuild.ParseResponseError(err);
							if ( responseMessage > '' ) {
								self.freeReportErrorMessage( responseMessage );
							}
							else {
								self.freeReportErrorMessage( "A problem has been occurred while submitting your data." );
							}
						}
					}
				} );
			}
			else
			{
				self.freeReportWarning( true );
			}
		};

		SignUp = ( data ): void => {
			window.location.assign( '/Manage/Subscription/' + data.Id );
		};

		//https://developer.wordpress.com/docs/api/1.1
		//https://public-api.wordpress.com/rest/v1.1/sites/gabcmissions.com/posts/?number=2
	}

	class SimplePriceOption {
		id: string = '';
		name: string = '';
		description: string = '';
		charge: number = 0;
		frequency: WarriorsGuild.Payments.Frequency;
		numberOfGuardians: number = 0;
		numberOfWarriors: number = 0;
		additionalGuardianCharge: number = 0;
		additionalWarriorCharge: number = 0;
		setupFee: number;
		perks: WarriorsGuild.Payments.Perk[] = [];
	}

	class DisplayablePriceOption {
		Id: string = '';
		Name: string = '';
		Description: string = '';
		Charge: CurrencyObservable<number>;
		Frequency: WarriorsGuild.Payments.Frequency;
		NumberOfGuardians: number = 0;
		NumberOfWarriors: number = 0;
		IncludedGuardiansAndWarriorsString: string;
		AdditionalGuardianCharge: CurrencyObservable<number>;
		AdditionalWarriorCharge: CurrencyObservable<number>;
		SetupFee: CurrencyObservable<number>;
		Perks: WarriorsGuild.Payments.Perk[] = [];
	}
}

WarriorsGuild.app.addViewModel( {
	name: "Home",
	bindingMemberName: "home",
	factory: WarriorsGuild.HomeViewModel,
	allowUnauthorized: true
} );
