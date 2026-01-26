declare var userIsAuthenticated: boolean;
//declare var oidcConfig: Oidc.UserManagerSettings;

namespace WarriorsGuild {
	export class AppViewModel {
		view: KnockoutObservable<{}>;
		Views: {
			Loading: {};
		};
		loading: KnockoutComputed<boolean>;
		dataModel: AppDataModel;
		_CSRF_TOKEN_KEY: string = "X-XSRF-Token";
		userManager: Oidc.UserManager = null;
		profileService: WarriorsGuild.ProfileService

		constructor(dataModel: AppDataModel) {
			var self = this;
			this.dataModel = dataModel;
			this.userManager = oidcManager;
			this.profileService = new WarriorsGuild.ProfileService()

			// Data
			this.Views = {
				Loading: {} // Other views are added dynamically by app.addViewModel(...).
			};

			
			// UI state
			this.view = ko.observable(this.Views.Loading);

			//var xhttp = new XMLHttpRequest();
			//xhttp.onreadystatechange = function () {
			//	if (xhttp.readyState == XMLHttpRequest.DONE) {
			//		if (xhttp.status == 200) {
			//			alert(xhttp.responseText);
			//		} else {
			//			alert('There was an error processing the AJAX request.');
			//		}
			//	}
			//};
			//xhttp.open('POST', '/api/password/changepassword', true);
			//xhttp.setRequestHeader("Content-type", "application/json");
			//xhttp.setRequestHeader("X-CSRF-TOKEN", csrfToken);
			//xhttp.send(JSON.stringify({ "newPassword": "ReallySecurePassword999$$$" }));
			this.loading = ko.computed(() => {
				return self.view() === self.Views.Loading;
			});
		}

		getUser = (): Oidc.User => {
			let result: Oidc.User = null;
			this.userManager.getUser().then(user => {
				result = user;
			})
			return result;
		}

		prepareAjax = (requireUser: boolean = true) => {
			WarriorsGuild.serviceBase.prepareAjax(requireUser);
		}

		// Private operations
		cleanUpLocation = (): void => {
			var self = this;
			window.location.hash = "";

			if (typeof history.pushState !== "undefined") {
				history.pushState("", document.title, location.pathname);
			}
		}

		// UI operations
		SetWarriorProfile = (warrior: Warrior): boolean => {
			if (warrior) {
				this.profileService.setWarriorProfile(warrior.id);
			}
			return true;
		}

		// UI operations
		TogglePreviewMode = (): void => {
			var self = this;
			this.profileService.togglePreviewMode(() => {
					window.location.reload();
				},
				(err: JQueryXHR) => {

				}
			);
		}

		login() {
			this.userManager.signinRedirect({ state: { user_callback_page: window.location } });
		};

		api() {
			let me = this;
			this.userManager.getUser().then(function (user) {
				//var url = "http://localhost:5000/identity";

				//var xhr = new XMLHttpRequest();
				//xhr.open("GET", url);
				//xhr.onload = function () {
				//	log(xhr.status, JSON.parse(xhr.responseText));
				//}
				//xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
				//xhr.send();
			});
		};

		logout() {
			this.userManager.signoutRedirect();
		};

		// Other navigateToX functions are added dynamically by app.addViewModel(...).

		// Other operations
		addViewModel(options): void {
			var me = this;
			var viewItem = new options.factory(me, me.dataModel),
				navigator;

			// Add view to AppViewModel.Views enum (for example, app.Views.Home).
			me[options.name] = viewItem;

			// Add binding member to AppViewModel (for example, app.home);
			me[options.bindingMemberName] = ko.computed(function () {
				if (!me.dataModel.getAccessToken()) {
					// The following code looks for a fragment in the URL to get the access token which will be
					// used to call the protected Web API resource
					var fragment = common.getFragment();

					if (fragment.access_token) {
						// returning with access token, restore old hash, or at least hide token
						window.location.hash = (fragment.state != null && fragment.state != '#_=_' ? fragment.state : '');
						//window.location.assign( window.location.protocol + '//' + window.location.host + fragment.state + ( fragment.hash || '' ) );
						me.dataModel.setAccessToken(fragment.access_token);
					}
					else {
						//TODO: Get Token
						//if ( userIsAuthenticated || !options.allowUnauthorized ) {
						//	var authorizeUrl = '/Account/Authorize?client_id=web&response_type=token&state={state}&hash={hash}&redirect_uri={redirectUrl}';
						//	authorizeUrl = authorizeUrl.replace( '{state}', encodeURIComponent( window.location.hash ) );
						//	authorizeUrl = authorizeUrl.replace( '{hash}', encodeURIComponent( window.location.hash ) );
						//	authorizeUrl = authorizeUrl.replace( '{redirectUrl}', encodeURIComponent( window.location.href.replace( window.location.hash, '' ) ) );

						//	// no token - so bounce to Authorize endpoint in AccountController to sign in or register
						//	window.location.assign( authorizeUrl );
						//}
					}
				}

				return me[options.name];
			});

			if (typeof options.navigatorFactory !== "undefined") {
				navigator = options.navigatorFactory(this, this.dataModel);
			} else {
				navigator = function () {
					window.location.hash = options.bindingMemberName;
				};
			}

			// Add navigation member to AppViewModel (for example, app.NavigateToHome());
			me["navigateTo" + options.name] = navigator;
		}

		initialize(): void {
			Sammy().run();
		}
	}

	export var app: AppViewModel;
	app = new AppViewModel(new AppDataModel());
}

function log(...args: Object[]) {
	//document.getElementById('results').innerText = '';

	Array.prototype.forEach.call(args, function (msg) {
		if (msg instanceof Error) {
			msg = "Error: " + msg.message;
		}
		else if (typeof msg !== 'string') {
			msg = JSON.stringify(msg, null, 2);
		}
		console.log(msg);
	});
}