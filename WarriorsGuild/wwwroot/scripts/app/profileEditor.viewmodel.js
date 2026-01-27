var WarriorsGuild;
(function (WarriorsGuild) {
    var ProfileEditorViewModel = /** @class */ (function () {
        function ProfileEditorViewModel(app, dataModel) {
            var _this = this;
            this.SaveProfile = function () {
                var self = _this;
                self.SavingProfile(true);
                self.SaveProfileFailure(false);
                self.SaveProfileSuccess(false);
                $.ajax({
                    url: profileEditorUrls.getProfile,
                    method: 'post',
                    data: $('#form1').serialize(),
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        self.SavingProfile(false);
                        self.SaveProfileSuccess(true);
                    },
                    error: function (err) {
                        self.SavingProfile(false);
                        self.SaveProfileFailure(false);
                    }
                });
            };
            this.getPaymentMethods = function () {
                var self = _this;
                $.ajax({
                    url: profileEditorUrls.paymentMethods,
                    method: 'get',
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data.paymentMethods, function (i, d) {
                            self.dataModel.PaymentMethods.push(self.MapToPaymentMethodObservable(d));
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: 'Failed to retrieve your saved payment methods'
                        });
                    }
                });
            };
            this.setDefaultPaymentMethod = function (data) {
                var self = _this;
                $.ajax({
                    url: profileEditorUrls.paymentMethods + '/' + data.id(),
                    method: 'put',
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        $.each(self.dataModel.PaymentMethods(), function (i, pm) {
                            pm.isDefault(data.id() === pm.id());
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Set Default Failure!",
                            message: 'Failed to update your default payment method' + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.deletePaymentMethod = function (data) {
                var self = _this;
                $.ajax({
                    url: profileEditorUrls.paymentMethods + '/' + data.id(),
                    method: 'delete',
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (d) {
                        self.dataModel.PaymentMethods.remove(data);
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Failed to Remove!",
                            message: 'Failed to remove the payment method' + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.MapToPaymentMethodObservable = function (p) {
                var self = _this;
                var mappedPm = ko.mapping.fromJS(p);
                return mappedPm;
            };
            this.prepareStripeControls = function (publishableKey) {
                var self = this;
                self.stripeControl = new WarriorsGuild.WgStripe();
                self.stripeControl.prepareStripeControls(publishableKey);
                self.stripeControl.config.tokenHandler = self.stripeTokenHandler;
            };
            this.stripeTokenHandler = function (token) {
                var self = _this;
                $.post({
                    url: '/api/PaymentMethods/?token=' + token,
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    }
                }).then(function (data) {
                    BootstrapAlert.success({
                        title: "Add Complete!",
                        message: "Your payment method was successfully created."
                    });
                    var oPM = self.MapToPaymentMethodObservable(data);
                    self.dataModel.PaymentMethods.push(oPM);
                    self.stripeControl.endSubmitSuccess();
                }, function (err, status) {
                    if (err.status === 404) {
                        BootstrapAlert.alert({
                            title: "Failed to add payment method!",
                            message: "Your payment method was not successfully created.  If you continue to have this problem, please notify us using the Contact page" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Failed to add payment method!",
                            message: "Your payment method was not successfully created.  Please try again" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                    self.stripeControl.endSubmitFailure(err.responseJSON != null ? '  ' + err.responseJSON.message : null);
                });
            };
            this.AddPaymentMethodSuccess = function (data) {
                var self = _this;
                //var model = self.createReadOnlySubscription( data );
                //self.dataModel.CurrentSubscription( model );
                BootstrapAlert.success({
                    title: "Add Complete!",
                    message: "Your payment method was successfully added!"
                });
            };
            this.handleAddPaymentMethodNotFound = function () {
                BootstrapAlert.alert({
                    title: "Add Failed!",
                    message: "Your payment method was not successfully added.  If you continue to have this problem, please notify us using the Contact page."
                });
            };
            this.handleAddPaymentMethodError = function () {
                BootstrapAlert.alert({
                    title: "Add Failed!",
                    message: "Your payment method was not successfully added.  Please try again."
                });
            };
            var self = this;
            this.app = app;
            this.SavingProfile = ko.observable(false);
            this.SaveProfileFailure = ko.observable(false);
            this.SaveProfileSuccess = ko.observable(false);
            this.SaveProfileResult = ko.computed(function () {
                return this.SaveProfileSuccess() ? 'Your profile has been successfully updated' :
                    this.SaveProfileFailure() ? 'Your profile update failed.  Please try again.' : '';
            }, this);
            this.imgSrcAttr = ko.observable(profileEditorUrls.getAvatarUrl);
            this.dataModel = {
                Id: ko.observable(''),
                Email: ko.observable(''),
                EmailConfirmed: ko.observable(''),
                FirstName: ko.observable(''),
                LastName: ko.observable(''),
                AddressLine1: ko.observable(''),
                AddressLine2: ko.observable(''),
                City: ko.observable(''),
                State: ko.observable(''),
                PostalCode: ko.observable(''),
                PhoneNumber: ko.observable(''),
                ShirtSize: ko.observable(''),
                UserName: ko.observable(''),
                PaymentMethods: ko.observableArray([]),
                AvatarUploadUrl: profileEditorUrls.avatarUploadUrl,
                HasAvatar: ko.pureComputed(function () { return false; }),
                AvatarSrc: ko.pureComputed(function () { return ''; }),
                Avatar: ko.observable(avatar),
                AvatarContentType: ko.observable(avatarContentType),
            };
            this.dataModel.HasAvatar = ko.pureComputed(function () {
                return this.dataModel.Avatar().length > 0;
            }, this);
            this.dataModel.AvatarSrc = ko.pureComputed(function () {
                if (this.dataModel.HasAvatar()) {
                    return "data:" + this.dataModel.AvatarContentType() + ';base64,' + this.dataModel.Avatar();
                }
                else {
                    return '';
                }
            }, this);
            app.prepareAjax();
            self.prepareStripeControls(stripePublishableKey);
            Sammy(function () {
                this.get('#profileEditor', function () {
                    self.SavingProfile(false);
                    self.SaveProfileFailure(false);
                    self.SaveProfileSuccess(false);
                    self.getPaymentMethods();
                });
                this.get('/Manage/EditProfile', function () {
                    this.app.runRoute('get', '#profileEditor');
                });
            });
        }
        return ProfileEditorViewModel;
    }());
    WarriorsGuild.ProfileEditorViewModel = ProfileEditorViewModel;
    var PaymentMethod = /** @class */ (function () {
        function PaymentMethod() {
        }
        return PaymentMethod;
    }());
    var ObservablePaymentMethod = /** @class */ (function () {
        function ObservablePaymentMethod() {
        }
        return ObservablePaymentMethod;
    }());
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "ProfileEditor",
    bindingMemberName: "profileEditor",
    factory: WarriorsGuild.ProfileEditorViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=profileEditor.viewmodel.js.map