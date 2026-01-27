var WarriorsGuild;
(function (WarriorsGuild) {
    var CrossDayViewModel = /** @class */ (function () {
        function CrossDayViewModel(params) {
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.
            var _this = this;
            this.baseUrl = '/api/crossStatus';
            this.isCollapsed = ko.observable(false);
            this.timeToUnlock = ko.pureComputed(function () {
                var _a;
                return new Date(2000, 1, 1);
                var d = (_a = _this.lastDayCompleted()) === null || _a === void 0 ? void 0 : _a.completedAt();
                if (d) {
                    var datePlus24 = new Date(d.getTime() + (24 * 60 * 60 * 1000));
                    return new Date(datePlus24.getFullYear(), datePlus24.getMonth(), datePlus24.getDate(), 6, 0, 0, 0);
                }
                else {
                    return null;
                }
            });
            this.isAnswerTextAreaEnabled = ko.pureComputed(function () {
                return _this.userIsWarrior
                    && (_this.crossDay === _this.nextDayToComplete() || _this.crossDay.editing())
                    && (new Date() > _this.timeToUnlock())
                    && (!_this.pendingApproval() || _this.crossDay.index() < _this.nextDayToComplete().index());
            });
            this.visible = ko.pureComputed(function () {
                return !_this.nextDayToComplete()
                    || (_this.crossDay.index() < _this.nextDayToComplete().index())
                    || (_this.crossDay === _this.nextDayToComplete() && _this.pendingApproval() !== null);
            });
            this.showComplete = ko.pureComputed(function () {
                return (_this.userIsWarrior && _this.crossDay.warriorCompleted()) || _this.crossDay.guardianReviewed();
            });
            this.saveDayAnswers = function () {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var answersToSave = [];
                $.each(_this.crossDay.questions(), function (index, value) {
                    var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                WarriorsGuild.serviceBase.put({
                    url: _this.baseUrl + "/".concat(self.cross().id(), "/day/").concat(_this.crossDay.id, "/answers"),
                    data: ko.toJSON(answersToSave),
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "Your answers have been saved"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "Your answers could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.completeDay = function () {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var answersToSave = [];
                $.each(_this.crossDay.questions(), function (index, value) {
                    var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.baseUrl, "/").concat(self.cross().id(), "/day/").concat(_this.crossDay.id, "/complete"),
                    data: ko.toJSON(answersToSave),
                    contentType: "application/json; charset=utf-8",
                    success: function (result) {
                        self.crossDay.completedAt(new Date());
                        if (self.crossDay.isCheckpoint()) {
                            var pendingApproval = new WarriorsGuild.PendingCrossApproval();
                            pendingApproval.dayId = self.crossDay.id;
                        }
                        self.answersSavedCallback(pendingApproval);
                        self.crossDay.editing(false);
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "The day has been completed"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The day could not be completed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.returnDay = function (data) {
                _this.crossDay.editing(true);
            };
            this.toggleCollapsed = function () {
                _this.isCollapsed(!_this.isCollapsed());
            };
            this.cross = params.cross;
            this.crossDay = params.crossDay;
            this.currentDay = params.currentDay;
            this.lastDayCompleted = params.lastDayCompleted;
            this.nextDayToComplete = params.nextDayToComplete;
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.pendingApproval = params.pendingApproval;
            this.answersSavedCallback = params.answersSavedCallback;
            this.isCollapsed(this.crossDay.warriorCompleted());
            this.showEntryField = ko.computed(function () {
                return _this.crossDay && (!_this.crossDay.warriorCompleted() || _this.crossDay.editing()) && _this.hasActiveSubscription;
            });
            this.enableEntryField = ko.computed(function () {
                return _this.crossDay && _this.crossDay.editing() && userIsWarrior;
            });
        }
        return CrossDayViewModel;
    }());
    WarriorsGuild.CrossDayViewModel = CrossDayViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('cross-day', {
    viewModel: WarriorsGuild.CrossDayViewModel,
    template: "\n                <div class=\"panel panel-default\" data-bind=\"if: crossDay && visible()\">\n                    <div class=\"panel-heading\">\n                        <h2 class=\"panel-title\" data-toggle=\"collapse\" data-bind=\"click: toggleCollapsed\">Passage <span data-bind=\"text: crossDay.index() + 1\"></span><span data-bind=\"visible: showComplete()\"> (Complete)</span></h2>\n                    </div>\n                    <div class=\"panel-collapse\" data-bind=\"css: { collapse: isCollapsed() }\">\n                        <div class=\"panel-body\">\n                            <!-- ko if: timeToUnlock() > new Date() -->\n\t\t\t\t\t\t        <!-- ko if: userIsWarrior -->\n\t\t\t\t\t\t            Available 6 AM tomorrow\n\t\t\t\t\t\t        <!-- /ko -->\n\t\t\t\t\t\t    <!-- /ko -->\n                            <!-- ko if: timeToUnlock() < new Date() -->\n                                <!-- ko if: userIsWarrior || crossDay.warriorCompleted() -->\n                                    <h4>Pray for focus and wisdom</h4>\n\t\t\t\t                    <h4>Read <span data-bind=\"text: crossDay.passage()\"></span></h4>\n\t\t\t\t                    <div class=\"row\" data-bind=\"foreach: crossDay.questions\">\n\t\t\t\t\t                    <cross-day-question-answer params=\"{cross: $component.cross,\n                                                                            crossDay: $component.crossDay,\n                                                                            question: $data,\n                                                                            hasActiveSubscription: $component.hasActiveSubscription,\n                                                                            userIsWarrior: $component.userIsWarrior,\n                                                                            showEntryField: $component.showEntryField\n                                                                            enableEntryField: $component.enableEntryField }\" />\n\t\t\t\t                    </div>\n\n\t\t\t\t                    <div class=\"row\" style=\"margin-top: 20px\">\n\t\t\t\t\t                    <div class=\"col-xs-12\">\n\t\t\t\t\t\t                    <button class=\"btn btn-sm btn-primary\" data-bind=\"click: function(data) { crossDay.editing(false); }, visible: crossDay.editing()\">Cancel Edit</button>\n\t\t\t\t\t\t                    <button class=\"btn btn-sm btn-primary btn-save-answers\" data-bind=\"click: completeDay, visible: isAnswerTextAreaEnabled()\">Save Answers</button>\n\t\t\t\t\t\t                    <button class=\"btn btn-sm btn-danger\" data-bind=\"click: returnDay, visible: userIsWarrior && crossDay.warriorCompleted() && !crossDay.editing()\">Edit Answers</button>\n                                            <div data-bind=\"visible: userIsWarrior && crossDay.warriorCompleted() && timeToUnlock() > new Date() && crossDay === lastDayCompleted()\">\n\t\t\t\t\t\t\t\t                <p>You have completed Day <span data-bind=\"text: $index() + 1\"></span>.</p>\n\t\t\t\t\t\t                    </div>\n\t\t\t\t\t                    </div>\n\t\t\t\t                    </div>\n\t\t\t\t\t\t        <!-- /ko -->\n                                <!-- ko if: !userIsWarrior && !crossDay.warriorCompleted() -->\n\t\t\t\t\t\t\t\t    <p>Awaiting Warrior Completion</p>                                \n\t\t\t\t\t\t        <!-- /ko -->\n\t\t\t\t\t\t    <!-- /ko -->\n                        </div>\n                    </div>\n                </div>\n"
});
//# sourceMappingURL=crossDay.component.js.map