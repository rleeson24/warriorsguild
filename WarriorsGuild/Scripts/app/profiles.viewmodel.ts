declare var profileUrls: {
	meUrl: string;
	registrationUrl: string;
	avatarUploadUrl: string;
	getAvatarUrl: string;
};

namespace WarriorsGuild {
	export class ProfilesViewModel {
		app: AppViewModel;
		dataModel: {
			MeUrl: string;
			RegistrationUrl: string;
			AvatarUploadUrl: string;
			GetAvatarUrl: string;
			me: ObservableUser;
			ChildUsers: KnockoutObservableArray<ApplicationUser>;
		};


		self: RingViewModel;
		constructor( app: WarriorsGuild.AppViewModel ) {
			this.app = app;
			var self = this;
			this.dataModel = {
				MeUrl: profileUrls.meUrl,
				RegistrationUrl: profileUrls.registrationUrl,
				AvatarUploadUrl: profileUrls.avatarUploadUrl,
				GetAvatarUrl: profileUrls.getAvatarUrl,
				me: new ObservableUser(),
				ChildUsers: ko.observableArray([])
			};
			app.prepareAjax();

			Sammy( function () {
				this.get( '#manageProfiles', function () {
					// Make a call to the protected Web API by passing in a Bearer Authorization Header
					$.ajax( {
						method: 'get',
						url: self.dataModel.MeUrl,
						contentType: "application/json; charset=utf-8",
						headers: {
							'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
						},
						success: function ( data: ManageProfilesModel ) {
							self.dataModel.me.id( data.me.id );
							self.dataModel.me.firstName( data.me.firstName );
							self.dataModel.me.lastName( data.me.lastName );
							self.dataModel.me.addressLine1( data.me.addressLine1 );
							self.dataModel.me.addressLine2( data.me.addressLine2 );
							self.dataModel.me.city( data.me.city );
							self.dataModel.me.state( data.me.state );
							self.dataModel.me.postalCode( data.me.postalCode );
							self.dataModel.me.phoneNumber( data.me.phoneNumber );
							self.dataModel.me.shirtSize( data.me.shirtSize );
							self.dataModel.me.email( data.me.email );
							self.dataModel.me.userName( data.me.userName );
							$.each( data.childUsers, function ( i, d ) {
								self.dataModel.ChildUsers.push( d );
							} );
							//$.each(data.childUsers, function (i, d) {
							//    var oRing = ko.mapping.fromJS(d, {
							//        copy: ["id"],
							//        requirements: {
							//            create: function (options) {
							//                var result = new ObservableRequirement();
							//                result.id = options.data.id;
							//                result.actionToComplete(options.data.actionToComplete);
							//                return result;
							//            }
							//        }
							//    });
							//    self.dataModel.Rings.push(oRing);
							//});
						}
					} );
				} );
				this.post( '', function () { return true; } );        //for image upload
				this.get( '/Manage/ManageProfiles', function () { this.app.runRoute( 'get', '#manageProfiles' ); } );
			} );
		}
		registerNewUser = (): void => {
			var self = this;
			$.ajax( {
				method: 'get',
				url: '/api/guardian/CanAddWarrior',
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function ( data: Boolean ) {
					if ( data ) {
						window.location.assign( self.dataModel.RegistrationUrl );
					}
					else {
						BootstrapAlert.warning( {
							title: "Insufficient Subscription",
							message: "You must update your subscription to add additional Warriors"
						} );
					}
				},
				error: function ( jqXHR ) {
					BootstrapAlert.alert( {
						title: "Unexpected error",
						message: "You must update your subscription to add additional Warriors"
					} );
				}
			} );
		};
	}
	class ManageProfilesModel {
		me: ApplicationUser;
		childUsers: ApplicationUser[];
	}
}

WarriorsGuild.app.addViewModel( {
	name: "Profiles",
	bindingMemberName: "profile",
	factory: WarriorsGuild.ProfilesViewModel
} );