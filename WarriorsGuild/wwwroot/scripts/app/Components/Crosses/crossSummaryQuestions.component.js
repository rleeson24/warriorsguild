var WarriorsGuild;
(function (WarriorsGuild) {
    var CrossSummaryQuestionsViewModel = /** @class */ (function () {
        function CrossSummaryQuestionsViewModel(params) {
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.
            var _this = this;
            this.isCollapsed = ko.observable(false);
            this.saveAnswers = function () {
                var self = _this;
                var answersToSave = [];
                $.each(_this.questions(), function (index, value) {
                    var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                _this.crossService.saveSummaryAnswers(_this.cross().id(), answersToSave, function (data) {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "Your answers have been saved"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "Your answers could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.completeCross = function () {
                var self = _this;
                if (_this.cross().warriorCompleted())
                    return;
                var answersToSave = [];
                $.each(_this.cross().questions(), function (index, value) {
                    var objToSave = { CrossId: self.cross().id(), CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                _this.crossService.completeCross(_this.cross().id(), answersToSave, function (result) {
                    self.cross().completedAt(new Date());
                    self.answersSavedCallback(result);
                    self.pendingApproval(result);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been completed"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be completed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.confirmCrossComplete = function () {
                var self = _this;
                _this.crossService.confirmCrossComplete(self.pendingApproval().approvalRecordId, function () {
                    self.pendingApproval(null);
                    self.cross().approvedAt(new Date());
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been confirmed"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be confirmed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.returnCross = function () {
                var self = _this;
                if (!_this.cross().warriorCompleted())
                    return;
                _this.crossService.returnCross(_this.cross().id(), function () {
                    self.cross().completedAt(null);
                    self.answersSavedCallback(100 - crossSummaryQuestionsWeight);
                    self.pendingApproval(null);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been returned"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.crossService = new WarriorsGuild.CrossService();
            this.cross = params.cross;
            this.questions = params.questions;
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.pendingApproval = params.pendingApproval;
            this.answersSavedCallback = params.answersSavedCallback;
            this.enableEntryField = params.questionsEnabled;
            this.showEntryField = params.showSummaryQuestionsEntryField;
        }
        return CrossSummaryQuestionsViewModel;
    }());
    WarriorsGuild.CrossSummaryQuestionsViewModel = CrossSummaryQuestionsViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('cross-summary-questions', {
    viewModel: WarriorsGuild.CrossSummaryQuestionsViewModel,
    template: "\n\t\t\t\t<div class=\"row\"><h3>Summary</h3></div>\n\t\t\t\t<div class=\"row\" data-bind=\"foreach: cross().questions\">\n\t\t\t\t\t<cross-day-question-answer params=\"{cross: $component.cross,\n                                                                    question: $data,\n                                                                    hasActiveSubscription: $component.hasActiveSubscription,\n                                                                    userIsWarrior: $component.userIsWarrior,\n                                                                    showEntryField: $component.showEntryField\n                                                                    }\" />\n\t\t\t\t</div>\n\n                <!-- ko if: hasActiveSubscription -->\n                    <!-- ko if: userIsWarrior -->\n\t\t\t\t\t    <div class=\"row\" style=\"margin-top: 20px\">\n\t\t\t\t\t\t\t<div class=\"col-xs-12\">\n\t\t\t\t\t\t\t\t<button class=\"btn btn-sm btn-primary\" data-bind=\"click: saveAnswers, visible: !cross().warriorCompleted() && $component.showEntryField()\">Save Answers</button>\n\t\t\t\t\t\t\t\t<button class=\"btn btn-sm btn-success\" data-bind=\"click: completeCross, visible: !cross().warriorCompleted() && $component.showEntryField()\">Submit for Review</button>\n\t\t\t\t\t\t\t\t<button class=\"btn btn-sm btn-danger\" data-bind=\"click: returnCross, visible: cross().warriorCompleted() && !cross().guardianReviewed()\">Recall Request for Review</button>\n\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t</div>\n                    <!-- /ko -->\n                    <!-- ko if: userIsGuardian -->\n\t\t\t\t\t    <div class=\"row\" style=\"margin-top: 20px\">\n\t\t\t\t\t\t\t<div class=\"col-xs-12\">\n\t\t\t\t\t\t\t\t<button class=\"btn btn-sm btn-primary\" data-bind=\"click: confirmCrossComplete, visible: cross().warriorCompleted() && !cross().guardianReviewed()\">Confirm Completion</button>\n\t\t\t\t\t\t\t\t<button class=\"btn btn-sm btn-danger\" data-bind=\"click: returnCross, visible: cross().warriorCompleted() && !cross().guardianReviewed()\">Return to Warrior</button>\n\t\t\t\t\t\t\t</div>\n\t\t\t\t\t\t</div>\n\t\t\t\t\t<!-- /ko -->\n                    <!-- ko if: cross().guardianReviewed() -->\n\t\t\t\t\t<div class=\"row\">\n\t\t\t\t\t\t<div class=\"col-xs-12\">\n\t\t\t\t\t\t\t<h3>Completed and Confirmed!</h3>\n\t\t\t\t\t\t</div>\n\t\t\t\t\t</div>\n\t\t\t\t\t<!-- /ko -->\n                <!-- /ko -->\n            "
});
//# sourceMappingURL=crossSummaryQuestions.component.js.map