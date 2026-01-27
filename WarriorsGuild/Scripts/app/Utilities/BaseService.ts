namespace WarriorsGuild {
    export class BaseService {
        _CSRF_TOKEN_KEY: string = "X-XSRF-Token";
        userManager: Oidc.UserManager = oidcManager;
        accessToken: string;
        siteUrl: string = "/";
        refreshToken: string;

        constructor() {
            var self = this;

            var ajaxHeaders = {};
            var VERIFICATION_TOKEN_CSS_SELECTOR = 'input[name="__RequestVerificationToken"]';
            if ($(VERIFICATION_TOKEN_CSS_SELECTOR).length > 0) {
                ajaxHeaders[this._CSRF_TOKEN_KEY] = $($(VERIFICATION_TOKEN_CSS_SELECTOR)[$(VERIFICATION_TOKEN_CSS_SELECTOR).length - 1]).val();
            }
            $(document).ajaxSend(function (event, request: JQueryXHR, settings) {
                if (!settings.crossDomain) {
                    request.setRequestHeader(self._CSRF_TOKEN_KEY, <string>$($(VERIFICATION_TOKEN_CSS_SELECTOR)[$(VERIFICATION_TOKEN_CSS_SELECTOR).length - 1]).val());
                }
            });
        }

		prepareAjax(requireUser: boolean = true): void {
			let self = this;
			this.userManager.getUser().then(function (user) {
				if (user) {
					$.ajaxSetup({
						beforeSend: function (xhr) {
							xhr.setRequestHeader("Authorization", `Bearer ${user.access_token}`);
						}
					})
					// register AJAX prefilter : options, original options
					$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
						let retryMax = 2;
						// retry not set or less than 2 : retry not requested
						//if (!originalOptions.retryMax || !originalOptions.retryMax >= 2) return;
						//// no timeout was setup
						//if (!originalOptions.timeout > 0) return;

						if (originalOptions['retryCount']) {
							// increment retry count each time
							originalOptions['retryCount']++;
						} else {
							// init the retry count if not set
							originalOptions['retryCount'] = 1;
							// copy original error callback on first time
							originalOptions['_error'] = originalOptions.error;
						};

						//// overwrite error handler for current request
						options.error = function (xhr, _textStatus, _errorThrown) {
							// retry max was exhausted or it is not a timeout error
							if (originalOptions['retryCount'] >= retryMax || (xhr.status !== 401)) {
								 //call original error handler if any
								if (originalOptions['_error']) originalOptions['_error'](xhr, _textStatus, _errorThrown);
								return;
							};
							//if (xhr.status === 401) {
							self.userManager.signinSilent().then(function (t) {
								// Call AJAX again with original options
								return $.ajax(originalOptions);
								//window.location = "/";
							}).catch(function (e) {
								console.error(e);
							});
						};
					});
					//self.dataModel.setAccessToken(user.access_token);
					//log("User logged in", user);
				}
				else {
					if (requireUser) {
						self.userManager.signinRedirect({ state: { user_callback_page: window.location } });
					}
					//log("User not logged in");
				}
			});
		}
		// Data access operations
		setAccessToken(accessToken): void {
			sessionStorage.setItem("accessToken", accessToken);
		};

        getAccessToken(): string {
            var self = this;
            var retrieving = false;
            while (retrieving) { }
            if (!this.accessToken) {
                retrieving = true;
                oidcManager.getUser().then(function (user) {
                    if (user) {
                        self.accessToken = user.access_token;
                        self.refreshToken = user.refresh_token;
						//self.dataModel.setAccessToken(user.access_token);
						log("User logged in");
						//log("User logged in", user);
                    }
                    else {
                        log("User not logged in");
                    }
                    retrieving = false;
                });
            }
            else {
                return this.accessToken;
            }
            //return sessionStorage.getItem( "accessToken" );
        };

        removeAccessToken(): void {
            sessionStorage.removeItem("accessToken");
        };

		put = (options) => {
			$.ajax({
				method: 'put',
				contentType: "application/json; charset=utf-8",
				...options
			})
		}

		post = (options) => {
			$.ajax({
				method: 'post',
				contentType: "application/json; charset=utf-8",
				...options
			})
		}

		get = (options) => {
			$.ajax({
				method: 'get',
				contentType: "application/json; charset=utf-8",
				...options
			})
		}
	}

	export const serviceBase = new BaseService();
}

//// register AJAX prefilter : options, original options
//$.ajaxPrefilter(function (options, originalOptions, jqXHR) {

//	originalOptions._error = originalOptions.error;

//	// overwrite error handler for current request
//	options.error = function (_jqXHR, _textStatus, _errorThrown) {

//		if (...it should not retry ...){

//	if (originalOptions._error) originalOptions._error(_jqXHR, _textStatus, _errorThrown);
//	return;
//};

//// else... Call AJAX again with original options
//$.ajax(originalOptions);
//   };
//});