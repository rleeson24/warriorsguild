var WarriorsGuild;
(function (WarriorsGuild) {
    var PaymentPlansViewModel = /** @class */ (function () {
        function PaymentPlansViewModel(app) {
            var _this = this;
            this.CreateObservablePriceOption = function (d) {
                var self = _this;
                var mappedPO = ko.mapping.fromJS(d, {
                    perks: {
                        create: function (options) {
                            return ko.mapping.fromJS(options.data, new ObservablePerk());
                        }
                    }
                }, new ObservablePriceOption());
                mappedPO.perks(mappedPO.perks() || []);
                mappedPO.charge = koCustom.currencyComputed(mappedPO.charge());
                mappedPO.setupFee = koCustom.currencyComputed(mappedPO.setupFee());
                mappedPO.additionalGuardianCharge = koCustom.currencyComputed(mappedPO.additionalGuardianCharge());
                mappedPO.additionalWarriorCharge = koCustom.currencyComputed(mappedPO.additionalWarriorCharge());
                return mappedPO;
            };
            this.HideAlerts = function () {
                _this.showDeleteSuccess(false);
                _this.showSaveSuccess(false);
                _this.showDeleteFailure(false);
                _this.showSaveFailure(false);
            };
            this.saveOption = function (data, saveUrl) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                _this.HideAlerts();
                var objectToSave;
                objectToSave = ko.toJS(data);
                $.each(objectToSave.perks, function (index, value) {
                    value.index = index;
                    value.priceOptionId = objectToSave.id;
                    if (value.id <= '')
                        delete value["id"];
                });
                var url = '';
                var method = '';
                if (objectToSave.id > '') {
                    url = saveUrl + '/' + data.id();
                    method = 'put';
                }
                else {
                    url = saveUrl;
                    method = 'post';
                }
                $.ajax({
                    method: method,
                    url: url,
                    data: ko.toJSON(objectToSave),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    error: function (err) {
                        self.actionFailureMessage('The Price Option could not be saved');
                        self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
                        self.showSaveFailure(true);
                    },
                    success: function (d) {
                        if (objectToSave.id === undefined) {
                            var mappedPO = self.CreateObservablePriceOption(d);
                            self.dataModel.PriceOptions.push(mappedPO);
                        }
                        else {
                            var oPo = ko.mapping.fromJS(d, {}, data);
                            self.dataModel.PriceOptions.push(oPo);
                        }
                        self.showSaveSuccess(true);
                    }
                });
            };
            this.activateOption = function (data, saveUrl) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                _this.HideAlerts();
                if (data.id() > '') {
                    var url = saveUrl + '/' + data.id() + "/Activate";
                    var method = 'put';
                    $.ajax({
                        method: method,
                        url: url,
                        contentType: "application/json; charset=utf-8",
                        headers: {
                            'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                        },
                        error: function (err) {
                            self.actionFailureMessage('The Price Option could not be activated');
                            self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
                            self.showSaveFailure(true);
                        },
                        success: function (returnData) {
                            data.stripeStatus(WarriorsGuild.WgStripeStatuses.STATUS_OPTION_ACTIVE);
                            self.showSaveSuccess(true);
                        }
                    });
                }
                else {
                    self.actionFailureMessage('The Price Option could not be activated.  Refresh the page and try again.');
                    self.showSaveFailure(true);
                }
            };
            this.deactivateOption = function (data, saveUrl) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                _this.HideAlerts();
                if (data.id() > '') {
                    var url = saveUrl + '/' + data.id() + "/Deactivate";
                    var method = 'put';
                    $.ajax({
                        method: method,
                        url: url,
                        contentType: "application/json; charset=utf-8",
                        headers: {
                            'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                        },
                        error: function (err) {
                            self.actionFailureMessage('The Price Option could not be deactivated');
                            self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
                            self.showSaveFailure(true);
                        },
                        success: function (returnData) {
                            data.stripeStatus(WarriorsGuild.WgStripeStatuses.STATUS_OPTION_INACTIVE);
                            self.showSaveSuccess(true);
                        }
                    });
                }
                else {
                    self.actionFailureMessage('The Price Option could not be deactivated.  Refresh the page and try again.');
                    self.showSaveFailure(true);
                }
            };
            this.deleteOption = function (data, saveUrl) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                _this.HideAlerts();
                var objectToSave = new WarriorsGuild.Payments.ManageablePriceOption();
                objectToSave.id = data.id();
                var url = saveUrl + '/' + data.id();
                var method = 'delete';
                $.ajax({
                    method: method,
                    url: url,
                    data: ko.toJSON(objectToSave),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    error: function (err) {
                        self.actionFailureMessage('The Price Option could not be deleted');
                        self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
                        self.showSaveFailure(true);
                    },
                    success: function (returnData) {
                        if (data.planId() <= '') {
                            self.dataModel.PriceOptions.remove(data);
                        }
                        else {
                            data.stripeStatus(WarriorsGuild.WgStripeStatuses.STATUS_OPTION_INCOMPLETE);
                        }
                        self.showSaveSuccess(true);
                    }
                });
            };
            this.app = app;
            var self = this;
            this.dataModel = {
                PriceOptionsUrl: paymentUrls.priceOptionsUrl,
                PriceOptions: ko.observableArray(),
                Name: ko.observable(''),
                Description: ko.observable(''),
                Frequency: ko.observable(0),
                Charge: koCustom.currencyComputed(0),
                SetupFee: koCustom.currencyComputed(0),
                NumberOfGuardians: koCustom.numericComputed(0),
                NumberOfWarriors: koCustom.numericComputed(0),
                AdditionalGuardianCharge: koCustom.currencyComputed(0),
                AdditionalWarriorCharge: koCustom.currencyComputed(0),
                HasTrialPeriod: ko.observable(false),
                TrialPeriodLength: koCustom.numericComputed(null),
                TrialPeriodCharge: koCustom.currencyComputed(null),
                perks: ko.observableArray([]),
                RetrievingPlans: ko.observable(false)
            };
            this.showDeleteSuccess = ko.observable(false);
            this.showSaveSuccess = ko.observable(false);
            this.showDeleteFailure = ko.observable(false);
            this.showSaveFailure = ko.observable(false);
            this.actionFailureMessage = ko.observable('');
            this.actionSuccessMessage = ko.observable('');
            app.prepareAjax();
            Sammy(function () {
                this.get('#priceOptions', function () {
                    self.HideAlerts();
                    self.dataModel.RetrievingPlans(true);
                    // Make a call to the protected Web API by passing in a Bearer Authorization Header
                    $.ajax({
                        method: 'get',
                        url: self.dataModel.PriceOptionsUrl,
                        contentType: "application/json; charset=utf-8",
                        headers: {
                            'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                        },
                        success: function (data) {
                            self.dataModel.PriceOptions.removeAll();
                            $.each(data, function (i, d) {
                                var mappedPO = self.CreateObservablePriceOption(d);
                                self.dataModel.PriceOptions.push(mappedPO);
                            });
                            self.dataModel.RetrievingPlans(false);
                        },
                        error: function (err) {
                            self.actionFailureMessage('The Price Options could not be retrieved');
                            self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
                            self.showSaveFailure(true);
                            self.dataModel.RetrievingPlans(false);
                        },
                    });
                });
                this.get('/ManagePriceOptions', function () { this.app.runRoute('get', '#priceOptions'); });
            });
        }
        return PaymentPlansViewModel;
    }());
    WarriorsGuild.PaymentPlansViewModel = PaymentPlansViewModel;
    var PriceOptionsDataModel = /** @class */ (function () {
        function PriceOptionsDataModel() {
        }
        return PriceOptionsDataModel;
    }());
    var ObservablePriceOption = /** @class */ (function () {
        function ObservablePriceOption() {
            var _this = this;
            this.id = ko.observable('');
            this.planId = ko.observable('');
            this.key = ko.observable('');
            this.name = ko.observable('');
            this.description = ko.observable('');
            this.stripeStatus = ko.observable(WarriorsGuild.WgStripeStatuses.STATUS_OPTION_INCOMPLETE);
            this.numberOfGuardians = ko.observable(0);
            this.numberOfWarriors = ko.observable(0);
            this.hasTrialPeriod = ko.observable(false);
            this.trialPeriodLength = ko.observable(null);
            this.trialPeriodCharge = ko.observable(null);
            this.perks = ko.observableArray([]);
            this.stripeStatusForDisplay = ko.computed(function () { return this.stripeStatus(); }, this);
            this.removePerk = function (data) {
                _this.perks.remove(data);
            };
        }
        ObservablePriceOption.prototype.addPerk = function (data) {
            data.perks.push(new ObservablePerk());
        };
        ;
        return ObservablePriceOption;
    }());
    var ObservablePerk = /** @class */ (function () {
        function ObservablePerk() {
            this.id = ko.observable('');
            this.description = ko.observable('');
            this.quantity = koCustom.numericComputed(null);
            this.priceOptionId = ko.observable('');
        }
        return ObservablePerk;
    }());
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "PaymentPlans",
    bindingMemberName: "paymentPlans",
    factory: WarriorsGuild.PaymentPlansViewModel
});
//# sourceMappingURL=paymentPlans.viewmodel.js.map