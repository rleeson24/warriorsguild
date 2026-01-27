var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
var WarriorsGuild;
(function (WarriorsGuild) {
    var BaseService = /** @class */ (function () {
        function BaseService() {
            this._CSRF_TOKEN_KEY = "X-XSRF-Token";
            this.userManager = oidcManager;
            this.siteUrl = "/";
            this.put = function (options) {
                $.ajax(__assign({ method: 'put', contentType: "application/json; charset=utf-8" }, options));
            };
            this.post = function (options) {
                $.ajax(__assign({ method: 'post', contentType: "application/json; charset=utf-8" }, options));
            };
            this.get = function (options) {
                $.ajax(__assign({ method: 'get', contentType: "application/json; charset=utf-8" }, options));
            };
            var self = this;
            var ajaxHeaders = {};
            var VERIFICATION_TOKEN_CSS_SELECTOR = 'input[name="__RequestVerificationToken"]';
            if ($(VERIFICATION_TOKEN_CSS_SELECTOR).length > 0) {
                ajaxHeaders[this._CSRF_TOKEN_KEY] = $($(VERIFICATION_TOKEN_CSS_SELECTOR)[$(VERIFICATION_TOKEN_CSS_SELECTOR).length - 1]).val();
            }
            $(document).ajaxSend(function (event, request, settings) {
                if (!settings.crossDomain) {
                    request.setRequestHeader(self._CSRF_TOKEN_KEY, $($(VERIFICATION_TOKEN_CSS_SELECTOR)[$(VERIFICATION_TOKEN_CSS_SELECTOR).length - 1]).val());
                }
            });
        }
        BaseService.prototype.prepareAjax = function (requireUser) {
            if (requireUser === void 0) { requireUser = true; }
            var self = this;
            this.userManager.getUser().then(function (user) {
                if (user) {
                    $.ajaxSetup({
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("Authorization", "Bearer ".concat(user.access_token));
                        }
                    });
                    // register AJAX prefilter : options, original options
                    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
                        var retryMax = 2;
                        // retry not set or less than 2 : retry not requested
                        //if (!originalOptions.retryMax || !originalOptions.retryMax >= 2) return;
                        //// no timeout was setup
                        //if (!originalOptions.timeout > 0) return;
                        if (originalOptions['retryCount']) {
                            // increment retry count each time
                            originalOptions['retryCount']++;
                        }
                        else {
                            // init the retry count if not set
                            originalOptions['retryCount'] = 1;
                            // copy original error callback on first time
                            originalOptions['_error'] = originalOptions.error;
                        }
                        ;
                        //// overwrite error handler for current request
                        options.error = function (xhr, _textStatus, _errorThrown) {
                            // retry max was exhausted or it is not a timeout error
                            if (originalOptions['retryCount'] >= retryMax || (xhr.status !== 401)) {
                                //call original error handler if any
                                if (originalOptions['_error'])
                                    originalOptions['_error'](xhr, _textStatus, _errorThrown);
                                return;
                            }
                            ;
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
        };
        // Data access operations
        BaseService.prototype.setAccessToken = function (accessToken) {
            sessionStorage.setItem("accessToken", accessToken);
        };
        ;
        BaseService.prototype.getAccessToken = function () {
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
        ;
        BaseService.prototype.removeAccessToken = function () {
            sessionStorage.removeItem("accessToken");
        };
        ;
        return BaseService;
    }());
    WarriorsGuild.BaseService = BaseService;
    WarriorsGuild.serviceBase = new BaseService();
})(WarriorsGuild || (WarriorsGuild = {}));
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
//# sourceMappingURL=BaseService.js.map