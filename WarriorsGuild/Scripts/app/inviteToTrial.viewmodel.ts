namespace WarriorsGuild {
	export class InviteToTrialViewModel {
		app: AppViewModel;
		dataModel: {
			emailAddress: KnockoutObservable<string>;
			Submitting: KnockoutObservable<boolean>;
		};
		submissionResult: KnockoutObservable<string>;
		constructor( app: WarriorsGuild.AppViewModel, dataModel ) {
			this.app = app;
			var self = this;
			this.dataModel = {
				Submitting: ko.observable<boolean>( false ),
				emailAddress: ko.observable<string>('')
			};
			self.submissionResult = ko.observable<string>('');
			app.prepareAjax();
			Sammy( function () {
				this.get( '/Invite', function () { this.app.runRoute( 'get', '/invite' ); } );
				this.get( '/invite', function () {} );
			} );
		}

		invite = (): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			$.ajax( {
				method: 'post',
				url: '/api/Product/Invite?emailAddress=' + self.dataModel.emailAddress(),
				data: ko.toJSON( { EmailAddress: self.dataModel.emailAddress() } ),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function () {
					BootstrapAlert.success( {
						title: "Guardian Invited!",
						message: "The Guardian has been successfully invited"
					} );
				},
				error: function ( err: JQueryXHR ) {
					BootstrapAlert.alert( {
						title: "Invitation Failed!",
						message: "The invitation process failed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					} );
				}
			} );
		}
	}
}

WarriorsGuild.app.addViewModel( {
	name: "InviteToTrial",
	bindingMemberName: "inviteToTrial",
	factory: WarriorsGuild.InviteToTrialViewModel,
	allowUnauthorized: false
} );
