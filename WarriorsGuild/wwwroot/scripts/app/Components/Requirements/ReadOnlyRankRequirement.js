var WarriorsGuild;
(function (WarriorsGuild) {
    var ReadOnlyRankRequirementViewModel = /** @class */ (function () {
        function ReadOnlyRankRequirementViewModel(params) {
            var _this = this;
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.
            this.actionToCompleteHtml = ko.computed(function () {
                return "".concat(_this.requirement.actionToCompleteLinked()).concat(_this.requirement.optional() ? ' (Optional)' : '');
            });
            this.isOutOfReach = ko.computed(function () {
                return userIsWarrior && !_this.requirement.warriorCompleted() && (_this.rank.requestPromotionEnabled() || _this.rank.pendingApproval());
            });
            this.guardianReviewed = ko.computed(function () { return _this.requirement.guardianReviewed(); });
            this.warriorCompleted = ko.computed(function () { return _this.requirement.warriorCompleted(); });
            this.completionSummaryText = ko.computed(function () { return _this.requirement.completionSummaryText(); });
            this.requireRing = ko.computed(function () { return _this.requirement.requireRing(); });
            this.savedRings = ko.computed(function () { return _this.requirement.savedRings(); });
            this.requireCross = ko.computed(function () { return _this.requirement.requireCross(); });
            this.requireAttachment = ko.computed(function () { return _this.requirement.requireAttachment(); });
            this.downloadProofOfCompletionFile = function (data) {
                WarriorsGuild.serviceBase.get({
                    url: "/api/rankstatus/ProofOfCompletion/OneUseFileKey/".concat(data.id),
                    contentType: "application/json; charset=utf-8",
                    success: function (oneTimeAccessToken) {
                        window.open("/api/rankstatus/ProofOfCompletion}/".concat(oneTimeAccessToken));
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.rank = params.rank;
            this.requirement = params.requirement;
            this.crossDay = params.crossDay;
            this.question = params.question;
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.showEntryField = true;
        }
        return ReadOnlyRankRequirementViewModel;
    }());
    WarriorsGuild.ReadOnlyRankRequirementViewModel = ReadOnlyRankRequirementViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('read-only-rank-req', {
    viewModel: WarriorsGuild.ReadOnlyRankRequirementViewModel,
    template: "<div class=\"col-xs-12 text-left col-requirement\">\n                    <div class=\"check\">\n                        <div class=\"squaredThree\" data-bind=\"css: { completed: guardianReviewed() }\">\n                            <div data-bind=\"attr: { id: 'rq' + $index(), checked: warriorCompleted() }\"></div>\n                            <label data-bind=\"attr: { for: 'rq' + $index(), title: completionSummaryText() }\"></label>\n                        </div>\n                    </div>\n                    <span class=\"actionToComplete\" data-bind=\"html: actionToCompleteHtml(), css: { outOfReach: isOutOfReach() }\"></span>\n                    <div style=\"width:100%; height: 100%; opacity:.5\"></div>\n                </div>\n                <div class=\"col-xs-12 col-sm-12 col-md-offset-2 col-md-8\" data-bind=\"visible: requireRing() && warriorCompleted()\">\n                    <div class=\"rankAdditionalRingRequirements\">\n                        <label>Rings:</label>\n                        <!-- ko foreach: savedRings() -->\n                            <img style=\"height:40px\" data-bind=\"attr: { src: imgSrcAttr, alt: name, title: name }\" />\n                        <!-- /ko -->\n                    </div>\n                </div>\n                <div class=\"col-xs-12 col-sm-12 col-md-offset-2 col-md-8\" data-bind=\"visible: requireCross() && warriorCompleted()\">\n                    <div class=\"row text-center\" data-bind=\"foreach: crossesToComplete\">\n                        <img style=\"height:40px\" data-bind=\"attr: { src: imgSrcAttr, alt: name, title: name }\" />\n                        <div data-bind=\"with: $parents[2].dataModel.DaysToComplete()[$index()]\">\n                            <cross-day params=\"{cross: $parentContext.$data,crossDay: $data,hasActiveSubscription: @HasActiveSubscription,userIsWarrior: @UserIsWarriorString }\" />\n                        </div>\n                    </div>\n                </div>\n                <div class=\"col-xs-12 col-sm-12 col-md-offset-2 col-md-8\" data-bind=\"visible: requireAttachment() && warriorCompleted()\">\n                    <div class=\"rankAdditionalRingRequirements\">\n                        <label>Attachments:</label>\n                        <!-- ko foreach: attachments -->\n                            <a data-bind=\"click: downloadProofOfCompletionFile\" style=\"cursor: pointer\" class=\"btn btn-sm btn-info\">\n                                <span data-bind=\"text:$index() + 1\"></span>\n                            </a>\n                        <!-- /ko -->\n                    </div>\n                </div>"
});
//# sourceMappingURL=ReadOnlyRankRequirement.js.map