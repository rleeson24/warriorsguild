var WarriorsGuild;
(function (WarriorsGuild) {
    var RequestPromotionDialogViewModel = /** @class */ (function () {
        function RequestPromotionDialogViewModel(params) {
            var _this = this;
            this.rank = ko.observable(null);
            this.isLoaded = ko.observable(false);
            this.submitForPromotion = function () {
                var self = _this;
                $('#requestPromotionPopup').modal('hide');
                WarriorsGuild.serviceBase.post({
                    url: "/api/rankstatus/SubmitForApproval/".concat(self.rank().id()),
                    contentType: "application/json; charset=utf-8",
                    success: function () {
                        BootstrapAlert.success({
                            title: "Action Success!",
                            message: "Rank submitted for promotion"
                        });
                        self.onSuccess();
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not submit this Rank for promotion"
                        });
                    }
                });
            };
            this.rank = params.rank;
            this.onSuccess = params.onSuccess;
            this.isLoaded(!this.isLoaded());
            this.RequestPromotionButtonText = ko.computed(function () {
                if (!_this.rank())
                    return '';
                _this.isLoaded();
                return _this.rank().completedPercent() == 100 ? 'Master'
                    : _this.rank().completedPercent() >= 66 ? 'Journeyman'
                        : _this.rank().completedPercent() >= 33 ? 'Apprentice'
                            : '';
            });
            this.SubmitForPromotionButtonText = ko.computed(function () {
                if (!_this.rank())
                    return '';
                _this.isLoaded();
                return _this.rank().completedPercent() < 100
                    ? 'Submit for Promotion'
                    : 'Request Round Table';
            });
        }
        return RequestPromotionDialogViewModel;
    }());
    WarriorsGuild.RequestPromotionDialogViewModel = RequestPromotionDialogViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('request-promotion-dialog', {
    viewModel: WarriorsGuild.RequestPromotionDialogViewModel,
    template: "<div class=\"modal fade\" id=\"requestPromotionPopup\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"modalLabel\" aria-hidden=\"true\">\n                    <div class=\"modal-dialog\" role=\"document\">\n                        <div class=\"modal-content\">\n                            <div class=\"modal-header\">\n                                <button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-label=\"Close\">\n                                    <span aria-hidden=\"true\">&times;</span>\n                                </button>\n                                <h4 class=\"modal-title\" id=\"modalLabel\">Request Promotion</h4>\n                            </div>\n                            <div class=\"modal-body\">\n                                <h1 style=\"width:100%;\">Congratulations!</h1>\n                                <h2 style=\"width:100%;\">You have completed enough of this Rank to level up to <span data-bind=\"text: RequestPromotionButtonText()\"></span>!</h2>\n                            </div>\n                            <div class=\"modal-footer\">\n                                <button class=\"btn btn-primary\" data-bind=\"click: submitForPromotion, text: SubmitForPromotionButtonText()\" data-dismiss=\"modal\"></button>\n                            </div>\n                        </div>\n                    </div>\n                </div>"
});
//# sourceMappingURL=requestPromotionDialog.js.map