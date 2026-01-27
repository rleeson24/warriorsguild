var WarriorsGuild;
(function (WarriorsGuild) {
    var InviteToTrialViewModel = /** @class */ (function () {
        function InviteToTrialViewModel(app, dataModel) {
            var _this = this;
            this.invite = function () {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                $.ajax({
                    method: 'post',
                    url: '/api/Product/Invite?emailAddress=' + self.dataModel.emailAddress(),
                    data: ko.toJSON({ EmailAddress: self.dataModel.emailAddress() }),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        BootstrapAlert.success({
                            title: "Guardian Invited!",
                            message: "The Guardian has been successfully invited"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Invitation Failed!",
                            message: "The invitation process failed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.app = app;
            var self = this;
            this.dataModel = {
                Submitting: ko.observable(false),
                emailAddress: ko.observable('')
            };
            self.submissionResult = ko.observable('');
            app.prepareAjax();
            Sammy(function () {
                this.get('/Invite', function () { this.app.runRoute('get', '/invite'); });
                this.get('/invite', function () { });
            });
        }
        return InviteToTrialViewModel;
    }());
    WarriorsGuild.InviteToTrialViewModel = InviteToTrialViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "InviteToTrial",
    bindingMemberName: "inviteToTrial",
    factory: WarriorsGuild.InviteToTrialViewModel,
    allowUnauthorized: false
});
//# sourceMappingURL=inviteToTrial.viewmodel.js.map