//declare var oidcConfig: Oidc.UserManagerSettings;
var WarriorsGuild;
(function (WarriorsGuild) {
    var AppViewModel = /** @class */ (function () {
        function AppViewModel(dataModel) {
            var _this = this;
            this._CSRF_TOKEN_KEY = "X-XSRF-Token";
            this.userManager = null;
            this.getUser = function () {
                var result = null;
                _this.userManager.getUser().then(function (user) {
                    result = user;
                });
                return result;
            };
            this.prepareAjax = function (requireUser) {
                if (requireUser === void 0) { requireUser = true; }
                WarriorsGuild.serviceBase.prepareAjax(requireUser);
            };
            // Private operations
            this.cleanUpLocation = function () {
                var self = _this;
                window.location.hash = "";
                if (typeof history.pushState !== "undefined") {
                    history.pushState("", document.title, location.pathname);
                }
            };
            // UI operations
            this.SetWarriorProfile = function (warrior) {
                if (warrior) {
                    _this.profileService.setWarriorProfile(warrior.id);
                }
                return true;
            };
            // UI operations
            this.TogglePreviewMode = function () {
                var self = _this;
                _this.profileService.togglePreviewMode(function () {
                    window.location.reload();
                }, function (err) {
                });
            };
            var self = this;
            this.dataModel = dataModel;
            this.userManager = oidcManager;
            this.profileService = new WarriorsGuild.ProfileService();
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
            this.loading = ko.computed(function () {
                return self.view() === self.Views.Loading;
            });
        }
        AppViewModel.prototype.login = function () {
            this.userManager.signinRedirect({ state: { user_callback_page: window.location } });
        };
        ;
        AppViewModel.prototype.api = function () {
            var me = this;
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
        ;
        AppViewModel.prototype.logout = function () {
            this.userManager.signoutRedirect();
        };
        ;
        // Other navigateToX functions are added dynamically by app.addViewModel(...).
        // Other operations
        AppViewModel.prototype.addViewModel = function (options) {
            var me = this;
            var viewItem = new options.factory(me, me.dataModel), navigator;
            // Add view to AppViewModel.Views enum (for example, app.Views.Home).
            me[options.name] = viewItem;
            // Add binding member to AppViewModel (for example, app.home);
            me[options.bindingMemberName] = ko.computed(function () {
                if (!me.dataModel.getAccessToken()) {
                    // The following code looks for a fragment in the URL to get the access token which will be
                    // used to call the protected Web API resource
                    var fragment = WarriorsGuild.common.getFragment();
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
            }
            else {
                navigator = function () {
                    window.location.hash = options.bindingMemberName;
                };
            }
            // Add navigation member to AppViewModel (for example, app.NavigateToHome());
            me["navigateTo" + options.name] = navigator;
        };
        AppViewModel.prototype.initialize = function () {
            Sammy().run();
        };
        return AppViewModel;
    }());
    WarriorsGuild.AppViewModel = AppViewModel;
    WarriorsGuild.app = new AppViewModel(new WarriorsGuild.AppDataModel());
})(WarriorsGuild || (WarriorsGuild = {}));
function log() {
    //document.getElementById('results').innerText = '';
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        args[_i] = arguments[_i];
    }
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
//# sourceMappingURL=app.viewmodel.js.map