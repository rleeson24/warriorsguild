var WarriorsGuild;
(function (WarriorsGuild) {
    var CrossViewModel = /** @class */ (function () {
        function CrossViewModel(params) {
            var _this = this;
            this.cross = ko.observable(new WarriorsGuild.ObservableCross());
            this.days = ko.observableArray([]);
            this.PendingApproval = ko.observable();
            this.totalPercentComplete = ko.pureComputed(function () {
                return _this.numberOfCompletedDays() * _this.percentWeightPerDay();
            });
            this.numberOfDays = ko.pureComputed(function () {
                return _this.days().length;
            });
            this.numberOfCompletedDays = ko.pureComputed(function () {
                return _this.days().filter(function (d) { return !!d.completedAt(); }).length;
            });
            this.percentWeightPerDay = ko.pureComputed(function () {
                var result = 0;
                if (_this.days() !== null) {
                    result = (100 - crossSummaryQuestionsWeight) / _this.numberOfDays();
                }
                return result;
            });
            this.lastDayCompleted = ko.pureComputed(function () {
                if (_this.days() === null) {
                    return null;
                }
                var lastCompleted = null;
                for (var d in _this.days()) {
                    var curr = _this.days()[d];
                    if (curr.warriorCompleted()) {
                        lastCompleted = curr;
                    }
                    else {
                        break;
                    }
                }
                return lastCompleted;
            });
            this.nextDayToComplete = ko.pureComputed(function () {
                if (_this.days() === null) {
                    return null;
                }
                var firstIncomplete = null;
                for (var d in _this.days()) {
                    var curr = _this.days()[d];
                    if (!curr.warriorCompleted()) {
                        firstIncomplete = curr;
                        break;
                    }
                }
                return firstIncomplete;
            });
            this.MAX_DAYS_TO_SHOW = 3;
            this.daysToShow = ko.pureComputed(function () {
                var currentDayIndex = _this.days().indexOf(_this.currentDay());
                var daysToShow = currentDayIndex > -1 ? 1 : 0;
                var previousDays = _this.days().slice(0, currentDayIndex);
                for (var i = previousDays.length - 1; i >= 0; i--) {
                    var item = previousDays[i];
                    if (!item.isCheckpoint())
                        daysToShow++;
                    else {
                        break;
                    }
                    if (!_this.showAllDaysSinceCheckpoint && daysToShow === _this.MAX_DAYS_TO_SHOW) {
                        break;
                    }
                }
                var startPosition = Math.max(currentDayIndex - (daysToShow - 1), 0);
                return _this.days().slice(startPosition, startPosition + daysToShow);
            });
            this.currentDay = ko.computed(function () {
                var result = null;
                if (_this.PendingApproval()) {
                    result = _this.lastDayCompleted();
                }
                else {
                    result = _this.nextDayToComplete();
                }
                return result;
            });
            this.allDaysComplete = function () {
                return !_this.days().some(function (d) { return d.completedAt() === null; });
            };
            this.summaryQuestionsEnabled = function () {
                return _this.summaryQuestionsVisible() && (_this.userIsWarrior || (!_this.userIsWarrior && _this.cross().warriorCompleted()));
            };
            this.summaryQuestionsVisible = function () {
                return (!_this.PendingApproval() || _this.PendingApproval().dayId === null) && _this.allDaysComplete();
            };
            this.showSummaryQuestionsEntryField = function () {
                return _this.userIsWarrior && _this.summaryQuestionsVisible() && !_this.PendingApproval();
            };
            this.getCrossDetail = function (id) {
                var self = _this;
                _this.crossService.getCrossDetail(id, function (data) {
                    ko.mapping.fromJS(data, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, self.cross());
                    self.retrieveCrossQuestions();
                    self.getCrossDays();
                    self.GetPendingApprovals();
                });
            };
            this.retrieveCrossQuestions = function () {
                var self = _this;
                _this.crossService.retrieveCrossQuestions(self.cross().id(), function (data) {
                    self.cross().questions.removeAll();
                    ko.mapping.fromJS(data, {
                        create: function (options) {
                            return self.CreateObservableQuestion(options.data);
                        }
                    }, self.cross().questions);
                });
            };
            this.getCrossDays = function () {
                var self = _this;
                self.days.removeAll();
                _this.crossService.getCrossDays(_this.cross().id(), function (data) {
                    $.each(data, function (i, day) {
                        var oDay = new WarriorsGuild.ObservableCrossDay();
                        oDay.passage(day.passage);
                        oDay.weight(day.weight);
                        oDay.isCheckpoint(day.isCheckpoint);
                        oDay.id = day.id;
                        oDay.index(day.index);
                        $.each(day.questions, function (j, q) {
                            var oq = new WarriorsGuild.ObservableCrossQuestion();
                            oq.id = q.id;
                            oq.text = q.text;
                            oq.answer(q.answer);
                            oDay.questions.push(oq);
                        });
                        oDay.approvedAt(WarriorsGuild.DateConversion.convertStringToNullableDate(day.approvedAt));
                        oDay.completedAt(WarriorsGuild.DateConversion.convertStringToNullableDate(day.completedAt));
                        self.days.push(oDay);
                    });
                    self.notifyParentOfPercentCompleteUpdated();
                });
            };
            this.answersSavedCallback = function (data) {
                var self = _this;
                self.PendingApproval(data);
                self.notifyParentOfPercentCompleteUpdated();
            };
            this.notifyParentOfPercentCompleteUpdated = function () {
                debounce(function () {
                    var self = _this;
                    if (self.completionPercentUpdatedHandler) {
                        if (!!self.PendingApproval() && self.PendingApproval().percentComplete > 0) {
                            self.completionPercentUpdatedHandler(self.PendingApproval().percentComplete);
                        }
                        else {
                            self.completionPercentUpdatedHandler(_this.totalPercentComplete());
                        }
                    }
                }, 300)();
            };
            this.GetPendingApprovals = function () {
                var self = _this;
                _this.crossService.getPendingApprovals(_this.cross().id(), function (data) {
                    if (data.length > 0) {
                        self.PendingApproval(data[0]);
                        self.notifyParentOfPercentCompleteUpdated();
                    }
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Retrieve Failure!",
                        message: "A problem has been occurred attempting to retrieve the pending cross approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.saveAnswers = function () {
                var self = _this;
                var answersToSave = [];
                $.each(_this.cross().questions(), function (index, value) {
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
                    self.completionPercentUpdatedHandler(_this.percentWeightPerDay() * _this.numberOfCompletedDays());
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
                    self.PendingApproval(result);
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
                _this.crossService.confirmCrossComplete(self.PendingApproval().approvalRecordId, function () {
                    self.PendingApproval(null);
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
            this.confirmCrossCheckpointComplete = function () {
                var self = _this;
                _this.crossService.confirmCrossCheckpointComplete(self.PendingApproval().approvalRecordId, function () {
                    self.PendingApproval(null);
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
                    self.PendingApproval(null);
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
            this.CreateObservableQuestion = function (data) {
                var self = _this;
                var result = new WarriorsGuild.ObservableCrossQuestion();
                result.id = data.id;
                result.crossId = self.cross().id();
                var formattedText = data.text.replace('{BookName}', self.cross().name());
                if (userIsWarrior || previewMode) {
                    formattedText = formattedText.replace('{explain}', self.cross().explainText());
                }
                result.text = formattedText;
                result.answer(data.answer);
                return result;
            };
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.
            this.crossId = params.crossId;
            this.crossService = new WarriorsGuild.CrossService();
            this.getCrossDetail(this.crossId());
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.userIsGuardian = params.userIsGuardian;
            this.totalCrossWeight = params.totalCrossWeight;
            this.completionPercentUpdatedHandler = params.completionPercentUpdatedHandler;
            this.showAllDaysSinceCheckpoint = params.showAllDaysSinceCheckpoint;
            this.crossId.subscribe(function (newValue) {
                _this.getCrossDetail(newValue);
            });
        }
        return CrossViewModel;
    }());
    WarriorsGuild.CrossViewModel = CrossViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('cross', {
    viewModel: WarriorsGuild.CrossViewModel,
    template: "\n\t\t\t\t<div data-bind=\"foreach: daysToShow()\" class=\"panel-group\">\n\t\t\t\t\t<cross-day params=\"{\n\t\t\t\t\t\t\t\t\t\t\tcross: ko.unwrap($component.cross()),\n\t\t\t\t\t\t\t\t\t\t\tcrossDay: $data,\n\t\t\t\t\t\t\t\t\t\t\tcurrentDay: $component.currentDay(),\n\t\t\t\t\t\t\t\t\t\t\tlastDayCompleted: $component.lastDayCompleted(),\n\t\t\t\t\t\t\t\t\t\t\tnextDayToComplete: $component.nextDayToComplete(),\n\t\t\t\t\t\t\t\t\t\t\thasActiveSubscription: $component.hasActiveSubscription,\n\t\t\t\t\t\t\t\t\t\t\tuserIsWarrior: $component.userIsWarrior,\n\t\t\t\t\t\t\t\t\t\t\tpendingApproval: $component.PendingApproval,\n\t\t\t\t\t\t\t\t\t\t\tanswersSavedCallback: $component.answersSavedCallback\n\t\t\t\t\t\t\t\t\t\t}\" />\n\t\t\t\t</div>\n\t\t\t\t<div class=\"row\" data-bind=\"if: !!PendingApproval()?.dayId\">\n\t\t\t\t\t<h4 data-bind=\"if: userIsWarrior\">You are progressing well! Time for a fireside chat with your guardian.</h4>\n\t\t\t\t\t<div class=\"col-xs-12\" data-bind=\"if: userIsGuardian\">\n\t\t\t\t\t\t<button class=\"btn btn-sm btn-primary\" data-bind=\"click: confirmCrossCheckpointComplete\">Fireside Chat Complete</button>\n\t\t\t\t\t</div>\n\t\t\t\t</div>\n\t\t\t\t<!-- ko if: summaryQuestionsVisible() -->\n\t\t\t\t\t<cross-summary-questions params=\"{\n\t\t\t\t\t\t\t\t\t\t\t\t\t\thasActiveSubscription: $component.hasActiveSubscription,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tuserIsWarrior: $component.userIsWarrior,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tuserIsGuardian: $component.userIsGuardian,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tcross: $component.cross,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tquestions: $component.cross().questions,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tpendingApproval: $component.PendingApproval,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tanswersSavedCallback: $component.answersSavedCallback,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tquestionsEnabled: $component.summaryQuestionsEnabled,\n\t\t\t\t\t\t\t\t\t\t\t\t\t\tshowSummaryQuestionsEntryField: $component.showSummaryQuestionsEntryField\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t}\"></cross-summary-questions>\n\t\t\t\t<!-- /ko -->\n\t\t\t"
});
//# sourceMappingURL=cross.component.js.map