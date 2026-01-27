var WarriorsGuild;
(function (WarriorsGuild) {
    var IntroViewModel = /** @class */ (function () {
        function IntroViewModel(app) {
            var _this = this;
            this.introScroll = function (vm, event) {
                var myDiv = event.currentTarget;
                if (myDiv.offsetHeight + myDiv.scrollTop >= myDiv.scrollHeight) {
                    _this.dataModel.introScrolledToBottom(true);
                }
            };
            this.signCovenant = function () {
                var self = _this;
                self.dataModel.Submitting(true);
                $.ajax({
                    method: 'post',
                    url: "/api/warrior/signcovenant",
                    contentType: "application/json; charset=utf-8",
                    data: '"' + self.dataModel.typedName + '"',
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        window.location.assign('/');
                    },
                    error: function (err) {
                        self.dataModel.Submitting(false);
                        BootstrapAlert.alert({
                            title: "Signing Failed!",
                            message: "Signing covenant failed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    },
                });
            };
            this.confirmVideoWatched = function () {
                var self = _this;
                document.getElementById('btnContinue').disabled = true;
                $.ajax({
                    method: 'post',
                    url: "/api/guardian/confirmVideoWatched",
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        window.location.assign('/');
                    },
                    error: function (err) {
                        document.getElementById('btnContinue').disabled = false;
                        BootstrapAlert.alert({
                            title: "Confirmation Failed!",
                            message: "Failed to confirm watching of video" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    },
                });
            };
            this.app = app;
            var self = this;
            this.dataModel = {
                typedName: ko.observable(),
                introScrolledToBottom: ko.observable(),
                signEnabled: ko.pureComputed(function () {
                    return _this.dataModel.introScrolledToBottom() && _this.dataModel.typedName() > '';
                }),
                Submitting: ko.observable(false)
            };
            app.prepareAjax();
            Sammy(function () {
                this.get('', function (routeParams) {
                });
            });
        }
        return IntroViewModel;
    }());
    WarriorsGuild.IntroViewModel = IntroViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Intro",
    bindingMemberName: "intro",
    factory: WarriorsGuild.IntroViewModel
});
//# sourceMappingURL=intro.viewmodel.js.map