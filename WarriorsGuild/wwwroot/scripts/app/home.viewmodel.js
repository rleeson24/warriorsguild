var WarriorsGuild;
(function (WarriorsGuild) {
    var HomeViewModel = /** @class */ (function () {
        function HomeViewModel(app, dataModel) {
            var _this = this;
            this.CreateDisplayablePriceOption = function (data) {
                var result = new DisplayablePriceOption();
                result.Id = data.id;
                result.Perks = data.perks;
                result.Description = data.description;
                result.NumberOfGuardians = data.numberOfGuardians;
                result.NumberOfWarriors = data.numberOfWarriors;
                result.Charge = koCustom.currencyComputed(data.charge);
                result.Name = data.name;
                result.AdditionalGuardianCharge = koCustom.currencyComputed(data.additionalGuardianCharge);
                result.AdditionalWarriorCharge = koCustom.currencyComputed(data.additionalWarriorCharge);
                result.Frequency = data.frequency;
                result.IncludedGuardiansAndWarriorsString = '(Includes ' + result.NumberOfGuardians + ' Guardian' + (result.NumberOfGuardians > 1 ? 's' : '') + ' & '
                    + result.NumberOfWarriors + ' Warrior' + (result.NumberOfWarriors > 1 ? 's' : '')
                    + ' login' + ((result.NumberOfGuardians + result.NumberOfWarriors) > 2 ? 's' : '') + ')';
                return result;
            };
            this.RequestFreeReport = function () {
                var self = _this;
                if (self.dataModel.EmailAddress() !== null && self.dataModel.EmailAddress().trim() > '') {
                    self.freeReportButtonEnabled(false);
                    self.freeReportError(false);
                    self.freeReportSuccess(false);
                    self.freeReportWarning(false);
                    // Make a call to the protected Web API by passing in a Bearer Authorization Header
                    $.ajax({
                        method: 'post',
                        url: 'api/MailingList/RequestFreeReport',
                        contentType: "application/json; charset=utf-8",
                        data: '"' + self.dataModel.EmailAddress() + '"',
                        success: function (data) {
                            self.dataModel.EmailAddress('');
                            self.freeReportButtonEnabled(true);
                            self.freeReportSuccess(true);
                        },
                        error: function (err) {
                            self.freeReportButtonEnabled(true);
                            self.freeReportError(true);
                            if (err.status === 409) {
                                self.freeReportErrorMessage("This free report has already been sent to this email address.");
                            }
                            else {
                                var responseMessage = WarriorsGuild.ParseResponseError(err);
                                if (responseMessage > '') {
                                    self.freeReportErrorMessage(responseMessage);
                                }
                                else {
                                    self.freeReportErrorMessage("A problem has been occurred while submitting your data.");
                                }
                            }
                        }
                    });
                }
                else {
                    self.freeReportWarning(true);
                }
            };
            this.SignUp = function (data) {
                window.location.assign('/Manage/Subscription/' + data.Id);
            };
            var self = this;
            this.dataModel = {
                EmailAddress: ko.observable(""),
                BlogEntries: ko.observableArray([]),
                PriceOptionsUrl: homeUrls.priceOptionsUrl,
                PriceOptions: ko.observableArray(),
                RetrievingPlans: ko.observable(false)
            };
            self.freeReportButtonEnabled = ko.observable(true);
            self.freeReportWarning = ko.observable(false);
            self.freeReportSuccess = ko.observable(false);
            self.freeReportError = ko.observable(false);
            self.freeReportErrorMessage = ko.observable('');
            self.retrievePlansFailure = ko.observable(false);
            this.dataModel.EmailAddress = ko.observable('');
            this.dataModel.BlogEntries = ko.observableArray([]);
            app.prepareAjax(false);
            Sammy(function () {
                this.get('#home', function () {
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
                    self.dataModel.RetrievingPlans(true);
                    self.retrievePlansFailure(false);
                    $.ajax({
                        method: 'get',
                        url: self.dataModel.PriceOptionsUrl + '/simple',
                        contentType: "application/json; charset=utf-8",
                        //headers: {
                        //	'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                        //},
                        success: function (data) {
                            self.dataModel.PriceOptions.removeAll();
                            $.each(data, function (i, d) {
                                var po = self.CreateDisplayablePriceOption(d);
                                self.dataModel.PriceOptions.push(po);
                            });
                            self.dataModel.RetrievingPlans(false);
                        },
                        error: function (err) {
                            self.retrievePlansFailure(true);
                            self.dataModel.RetrievingPlans(false);
                        },
                    });
                });
                this.get('/', function () { this.app.runRoute('get', '#home'); });
            });
        }
        return HomeViewModel;
    }());
    WarriorsGuild.HomeViewModel = HomeViewModel;
    var SimplePriceOption = /** @class */ (function () {
        function SimplePriceOption() {
            this.id = '';
            this.name = '';
            this.description = '';
            this.charge = 0;
            this.numberOfGuardians = 0;
            this.numberOfWarriors = 0;
            this.additionalGuardianCharge = 0;
            this.additionalWarriorCharge = 0;
            this.perks = [];
        }
        return SimplePriceOption;
    }());
    var DisplayablePriceOption = /** @class */ (function () {
        function DisplayablePriceOption() {
            this.Id = '';
            this.Name = '';
            this.Description = '';
            this.NumberOfGuardians = 0;
            this.NumberOfWarriors = 0;
            this.Perks = [];
        }
        return DisplayablePriceOption;
    }());
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Home",
    bindingMemberName: "home",
    factory: WarriorsGuild.HomeViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=home.viewmodel.js.map