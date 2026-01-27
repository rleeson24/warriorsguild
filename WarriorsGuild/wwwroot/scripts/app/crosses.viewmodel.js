var WarriorsGuild;
(function (WarriorsGuild) {
    var CrossesViewModel = /** @class */ (function () {
        function CrossesViewModel(app) {
            var _this = this;
            this.crossId = ko.observable();
            this.retrieveDays = function (crossId) {
                var self = _this;
                _this.crossService.getCrossDays(crossId, function (data) {
                    self.crossDetail.days.removeAll();
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
                        self.crossDetail.days.push(oDay);
                    });
                    self.GetPendingApprovals();
                });
            };
            this.retrieveDayQuestions = function () {
                var self = _this;
                _this.crossService.retrieveTemplateQuestions(function (data) {
                    if (self.crossDetail.days().length > 0) {
                        for (var i = 0; i < self.crossDetail.days().length; i++) {
                            self.crossDetail.days()[i].questions.removeAll();
                            ko.mapping.fromJS(data, {
                                create: function (options) {
                                    return self.CreateObservableQuestion(self.crossDetail, options.data);
                                }
                            }, self.crossDetail.days()[i].questions);
                        }
                    }
                });
            };
            this.retrieveCrossQuestions = function () {
                var self = _this;
                _this.crossService.retrieveCrossQuestions(self.crossDetail.id(), function (data) {
                    self.crossDetail.questions.removeAll();
                    ko.mapping.fromJS(data, {
                        create: function (options) {
                            return self.CreateObservableQuestion(self.crossDetail, options.data);
                        }
                    }, self.crossDetail.questions);
                });
            };
            this.GetPendingApprovals = function () {
                var self = _this;
                _this.crossService.getPendingApprovals(self.crossDetail.id(), function (data) {
                    self.dataModel.PendingCrossApprovals(data);
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Retrieve Failure!",
                        message: "A problem has been occurred attempting to retrieve the pending cross approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.retrievePinnedCrosses = function () {
                var self = _this;
                _this.crossService.getPinnedCrosses(function (data) {
                    $.each(data, function (i, val) {
                        $.each(self.dataModel.Crosses(), function (j, cross) {
                            if (cross.id() === val.crossId) {
                                cross.isPinned(true);
                                return false;
                            }
                        });
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Pins Retrieval Failed!",
                        message: "Could not retrieve pins"
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                });
            };
            this.retrieveCompletedCrosses = function () {
                var self = _this;
                _this.crossService.getCompletedCrosses(function (data) {
                    $.each(data, function (i, val) {
                        $.each(self.dataModel.Crosses(), function (j, cross) {
                            if (cross.id() === val.id) {
                                cross.isCompleted(true);
                                return false;
                            }
                        });
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Completed Ring Retrieval Failed!",
                        message: "Could not retrieve completed crosses"
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                });
            };
            this.saveNewCrossOrder = function (data) {
                var self = _this;
                var crossOrder = [];
                $.each(self.dataModel.Crosses(), function (index, value) {
                    var objToSave = { Id: value.id(), Index: index + 1 };
                    crossOrder.push(objToSave);
                });
                _this.crossService.updateCrossOrder(crossOrder, function (data) {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "Cross Order has been updated"
                    });
                }, function (err) {
                    data.cancelDrop = true;
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "Cross order could not be updated"
                    });
                });
            };
            this.saveCross = function () {
                var self = _this;
                _this.crossService.createCross({ Name: _this.dataModel.Name(), Description: _this.dataModel.Description() }, function (data) {
                    var oCross = ko.mapping.fromJS(data, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, new WarriorsGuild.ObservableCross());
                    self.dataModel.Crosses.push(oCross);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The Cross has been saved"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The Cross save process failed"
                    });
                });
            };
            this.updateCross = function (data) {
                var self = _this;
                _this.crossService.updateCross({ Id: data.id(), Name: data.name(), Description: data.description(), ExplainText: data.explainText() }, function (data) {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The Cross has been saved"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The Cross could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.saveCrossDaysAndQuestions = function (data) {
                _this.updateCross(data);
                _this.saveDays(data);
            };
            //updateCrossQuestions = (data: ObservableCross): void => {
            //	var self = this;
            //	// Make a call to the protected Web API by passing in a Bearer Authorization Header
            //	var questionsToSave = [];
            //	$.each(data.questions(), function (index, value) {
            //		var objToSave = { CrossId: data.id(), Text: value.text(), Index: index };
            //		if (value.id !== '') objToSave['Id'] = value.id;
            //		questionsToSave.push(objToSave);
            //	});
            //	$.ajax({
            //		method: 'put',
            //		url: self.dataModel.CrossesUrl + '/' + data.id() + '/questions',
            //		data: ko.toJSON(questionsToSave),
            //		contentType: "application/json; charset=utf-8",
            //		headers: {
            //			'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
            //		},
            //		success: function (data: Cross) {
            //			BootstrapAlert.success({
            //				title: "Save Success!",
            //				message: "The Cross has been saved"
            //			});
            //		},
            //		error: function (err: JQueryXHR) {
            //			BootstrapAlert.alert({
            //				title: "Save Failed!",
            //				message: "The Cross could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
            //			});
            //		}
            //	});
            //}
            this.saveDays = function (data) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var daysToSave = [];
                $.each(data.days(), function (index, value) {
                    var objToSave = { Passage: value.passage(), Weight: value.weight(), isCheckpoint: value.isCheckpoint(), index: index, id: value.id };
                    if (value.id !== '')
                        objToSave['DayId'] = value.id;
                    daysToSave.push(objToSave);
                });
                _this.crossService.updatePassages(data.id(), daysToSave, function (data) {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The days have been saved"
                    });
                    self.crossDetail.days.removeAll();
                    $.each(data, function (i, day) {
                        var oDay = new WarriorsGuild.ObservableCrossDay();
                        oDay.passage(day.passage);
                        oDay.weight(day.weight);
                        oDay.isCheckpoint(day.isCheckpoint);
                        oDay.id = day.id;
                        self.crossDetail.days.push(oDay);
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The days could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.saveDayAnswers = function (data) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var answersToSave = [];
                $.each(data.questions(), function (index, value) {
                    var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                _this.crossService.savePassageAnswers(self.crossDetail.id(), data.id, answersToSave, function (data) {
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
            this.saveAnswers = function (data) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var answersToSave = [];
                $.each(data.questions(), function (index, value) {
                    var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                _this.crossService.saveSummaryAnswers(data.id(), answersToSave, function (data) {
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
            this.completeDay = function (data) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var answersToSave = [];
                $.each(data.questions(), function (index, value) {
                    var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                _this.crossService.completeCrossDay(self.crossDetail.id(), data.id, answersToSave, function () {
                    data.completedAt(new Date());
                    if (data.isCheckpoint()) {
                        var pendingApproval = new WarriorsGuild.PendingCrossApproval();
                        pendingApproval.dayId = data.id;
                        self.dataModel.PendingCrossApprovals.push(pendingApproval);
                    }
                    data.editing(false);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The day has been completed"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The day could not be completed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.returnDay = function (data) {
                data.editing(true);
                //$.ajax({
                //	method: 'post',
                //	url: `${crossUrls.crossStatusUrl}/${self.crossDetail.id()}/day/${data.id}/return`,
                //	contentType: "application/json; charset=utf-8",
                //	headers: {
                //		'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                //	},
                //	success: function () {
                //		data.completedAt(null);
                //		BootstrapAlert.success({
                //			title: "Save Success!",
                //			message: "This day has been returned"
                //		});
                //	},
                //	error: function (err: JQueryXHR) {
                //		BootstrapAlert.alert({
                //			title: "Save Failed!",
                //			message: "The day could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                //		});
                //	}
                //});
            };
            this.completeCross = function (data) {
                var self = _this;
                if (data.warriorCompleted())
                    return;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var answersToSave = [];
                $.each(data.questions(), function (index, value) {
                    var objToSave = { CrossId: data.id(), CrossQuestionId: value.id, Answer: value.answer() };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    answersToSave.push(objToSave);
                });
                _this.crossService.completeCross(data.id(), answersToSave, function (result) {
                    data.completedAt(new Date());
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
            this.returnCross = function (data) {
                var self = _this;
                if (!data.warriorCompleted())
                    return;
                _this.crossService.returnCross(data.id(), function () {
                    data.completedAt(null);
                    self.dataModel.PendingCrossApprovals.removeAll();
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
            this.confirmCrossComplete = function (data) {
                var self = _this;
                _this.crossService.confirmCrossComplete(self.dataModel.PendingCrossApprovals()[0].approvalRecordId, function () {
                    self.dataModel.PendingCrossApprovals.removeAll();
                    self.crossDetail.approvedAt(new Date());
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
            this.confirmCrossDayComplete = function (data) {
                var self = _this;
                _this.crossService.confirmCrossDayComplete(self.dataModel.PendingCrossApprovals()[0].approvalRecordId, function () {
                    self.dataModel.PendingCrossApprovals.removeAll();
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
            this.pin = function (data) {
                var self = _this;
                _this.crossService.pinCross(data.id(), function (ring) {
                    data.isPinned(true);
                    BootstrapAlert.success({
                        title: "Pin Success!",
                        message: "The cross has been pinned"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Pin Failed!",
                        message: "Could not pin the cross"
                    });
                });
            };
            this.unpin = function (data) {
                var self = _this;
                _this.crossService.unpinCross(data.id(), function (ring) {
                    data.isPinned(false);
                    BootstrapAlert.success({
                        title: "Unpin Success!",
                        message: "The cross has been un-pinned"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Unpin Failed!",
                        message: "Could not un-pin the cross"
                    });
                });
            };
            this.CreateObservableQuestion = function (cross, data) {
                var self = _this;
                var result = new WarriorsGuild.ObservableCrossQuestion();
                result.id = data.id;
                result.crossId = cross.id();
                var formattedText = data.text.replace('{BookName}', cross.name());
                if (userIsWarrior || previewMode) {
                    formattedText = formattedText.replace('{explain}', cross.explainText());
                }
                result.text = formattedText;
                result.answer(data.answer);
                return result;
            };
            this.isAnswerTextAreaEnabled = function (userIsWarrior, day) {
                return userIsWarrior
                    && ((day === _this.dataModel.nextDayToComplete()
                        && new Date() > _this.dataModel.timeToUnlock())
                        || (day.editing() && day.warriorCompleted()))
                    && (_this.dataModel.PendingCrossApprovals().length === 0 || day.index() < _this.dataModel.nextDayToComplete().index());
            };
            this.summaryQuestionsEnabled = function (userIsWarrior) {
                return userIsWarrior && _this.dataModel.PendingCrossApprovals().length === 0 && !_this.crossDetail.days().some(function (d) { return d.completedAt() === null; });
            };
            this.summaryQuestionsVisible = function (userIsWarrior) {
                return (userIsWarrior && !_this.crossDetail.days().some(function (d) { return d.completedAt() === null; }) && _this.dataModel.PendingCrossApprovals().length === 0)
                    || (!userIsWarrior && _this.crossDetail.warriorCompleted());
            };
            this.app = app;
            this.user = app.getUser();
            this.crossService = new WarriorsGuild.CrossService();
            var self = this;
            this.dataModel = {
                CrossesUrl: crossUrls.crossesUrl,
                crossDetailUrl: crossUrls.detailUrl,
                Crosses: ko.observableArray(),
                Name: ko.observable(''),
                Description: ko.observable(''),
                imageUploadUrl: crossUrls.imageUploadBaseUrl,
                imageBaseUrl: crossUrls.imageBaseUrl,
                selectedWarrior: app.dataModel.selectedWarrior,
                PendingCrossApprovals: ko.observableArray([]),
                lastDayCompleted: ko.pureComputed(function () {
                    if (_this.crossDetail === null || _this.crossDetail.days() === null) {
                        return null;
                    }
                    var lastCompleted = null;
                    for (var d in _this.crossDetail.days()) {
                        var curr = _this.crossDetail.days()[d];
                        if (curr.warriorCompleted()) {
                            lastCompleted = curr;
                        }
                        else {
                            break;
                        }
                    }
                    return lastCompleted;
                }),
                nextDayToComplete: ko.pureComputed(function () {
                    if (_this.crossDetail === null || _this.crossDetail.days() === null) {
                        return null;
                    }
                    var firstIncomplete = null;
                    for (var d in _this.crossDetail.days()) {
                        var curr = _this.crossDetail.days()[d];
                        if (!curr.warriorCompleted()) {
                            firstIncomplete = curr;
                            break;
                        }
                    }
                    return firstIncomplete;
                }),
                timeToUnlock: ko.pureComputed(function () {
                    var _a;
                    var d = (_a = _this.dataModel.lastDayCompleted()) === null || _a === void 0 ? void 0 : _a.completedAt();
                    if (d) {
                        var datePlus24 = new Date(d.getTime() + (24 * 60 * 60 * 1000));
                        return new Date(datePlus24.getFullYear(), datePlus24.getMonth(), datePlus24.getDate(), 6, 0, 0, 0);
                    }
                    else {
                        return null;
                    }
                })
            };
            this.DetailView = ko.observable(false);
            this.crossDetail = new WarriorsGuild.ObservableCross();
            this.PreviewMode = ko.observable(previewMode);
            this.ReturnToListView = function () {
                self.DetailView(false);
            };
            app.prepareAjax(false);
            Sammy(function () {
                this.get('#crosses', function () {
                    if (canViewAll) {
                        self.crossService.getCrossList(function (data) {
                            self.dataModel.Crosses.removeAll();
                            $.each(data, function (i, d) {
                                var oCross = ko.mapping.fromJS(d, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, new WarriorsGuild.ObservableCross());
                                oCross.pin = self.pin;
                                oCross.unpin = self.unpin;
                                self.dataModel.Crosses.push(oCross);
                            });
                            self.retrievePinnedCrosses();
                            self.retrieveCompletedCrosses();
                        });
                        self.DetailView(false);
                    }
                    else {
                        self.crossService.getPublicCross(function (data) {
                            self.crossDetail.questions.removeAll();
                            ko.mapping.fromJS(data, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, self.crossDetail);
                            if (data != null) {
                                self.DetailView(true);
                            }
                        });
                    }
                });
                this.post('', function () { return true; }); //for image upload
                this.get('/crosses', function () { this.app.runRoute('get', '#crosses'); });
                this.get('/Crosses', function () { this.app.runRoute('get', '#crosses'); });
                this.get('/Crosses#detail', function () { this.app.runRoute('get', '/crosses#detail', this.params); });
                this.get('/crosses#detail', function () {
                    var currContext = this;
                    self.crossId(currContext.params.id);
                    if (self.dataModel.Crosses().length === 0) {
                        self.crossService.getCrossDetail(currContext.params.id, function (data) {
                            self.crossDetail.questions.removeAll();
                            self.crossDetail.days.removeAll();
                            ko.mapping.fromJS(data, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, self.crossDetail);
                            self.retrieveDays(currContext.params.id);
                            self.retrieveCrossQuestions();
                        });
                    }
                    else {
                        self.crossDetail.questions.removeAll();
                        $.each(self.dataModel.Crosses(), function (i, e) {
                            if (e.id() === currContext.params.id) {
                                var crossToEdit = ko.mapping.toJS(e);
                                ko.mapping.fromJS(crossToEdit, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, self.crossDetail);
                                self.retrieveDays(currContext.params.id);
                                self.retrieveCrossQuestions();
                                return false;
                            }
                        });
                    }
                    self.DetailView(true);
                });
            });
        }
        return CrossesViewModel;
    }());
    WarriorsGuild.CrossesViewModel = CrossesViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Crosses",
    bindingMemberName: "crosses",
    factory: WarriorsGuild.CrossesViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=crosses.viewmodel.js.map