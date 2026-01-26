declare var crossUrls: {
    crossesUrl: string;
    crossStatusUrl: string;
    publicCrossUrl: string;
    detailUrl: string;
    recordCompletion: string;
    imageUploadBaseUrl: string;
    imageBaseUrl: string;
    uploadGuideUrl: string;
    downloadGuideUrl: string;
};
declare var isLoggedIn: boolean;
declare var canViewAll: boolean;
declare var previewMode: boolean;
declare var userIsWarrior: boolean;

namespace WarriorsGuild {
    export class CrossesViewModel {
        app: AppViewModel;
        crossService: WarriorsGuild.CrossService;

        ReturnToListView: () => void;
        DetailView: KnockoutObservable<boolean>;
        crossDetail: ObservableCross;
        PreviewMode: KnockoutObservable<boolean>;

        dataModel: {
            CrossesUrl: string;
            crossDetailUrl: string;
            imageUploadUrl: string;
            imageBaseUrl: string;
            Crosses: KnockoutObservableArray<ObservableCross>;
            Name: KnockoutObservable<string>;
            Description: KnockoutObservable<string>;
            selectedWarrior: KnockoutObservable<Warrior>;
            lastDayCompleted: KnockoutComputed<ObservableCrossDay | null>;
            nextDayToComplete: KnockoutComputed<ObservableCrossDay | null>;
            timeToUnlock: KnockoutComputed<Date | null>;
            PendingCrossApprovals: KnockoutObservableArray<PendingCrossApproval>;
        };
        crossId: KnockoutObservable<string> = ko.observable<string>();
        user: Oidc.User;
        constructor(app: WarriorsGuild.AppViewModel) {
            this.app = app;
            this.user = app.getUser();
            this.crossService = new WarriorsGuild.CrossService();
            var self = this;
            this.dataModel = {
                CrossesUrl: crossUrls.crossesUrl,
                crossDetailUrl: crossUrls.detailUrl,
                Crosses: ko.observableArray<ObservableCross>(),
                Name: ko.observable(''),
                Description: ko.observable(''),
                imageUploadUrl: crossUrls.imageUploadBaseUrl,
                imageBaseUrl: crossUrls.imageBaseUrl,
                selectedWarrior: app.dataModel.selectedWarrior,
                PendingCrossApprovals: ko.observableArray<PendingCrossApproval>([]),
                lastDayCompleted: ko.pureComputed<ObservableCrossDay | null>(() => {
                    if (this.crossDetail === null || this.crossDetail.days() === null) {
                        return null;
                    }
                    let lastCompleted = null;
                    for (let d in this.crossDetail.days()) {
                        let curr = this.crossDetail.days()[d];
                        if (curr.warriorCompleted()) {
                            lastCompleted = curr;
                        }
                        else {
                            break;
                        }
                    }
                    return lastCompleted;
                }),
                nextDayToComplete: ko.pureComputed<ObservableCrossDay | null>(() => {
                    if (this.crossDetail === null || this.crossDetail.days() === null) {
                        return null;
                    }
                    let firstIncomplete = null;
                    for (let d in this.crossDetail.days()) {
                        let curr = this.crossDetail.days()[d];
                        if (!curr.warriorCompleted()) {
                            firstIncomplete = curr;
                            break;
                        }
                    }
                    return firstIncomplete;
                }),
                timeToUnlock: ko.pureComputed<Date | null>(() => {
                    let d = this.dataModel.lastDayCompleted()?.completedAt();
                    if (d) {
                        var datePlus24 = new Date(d.getTime() + (24 * 60 * 60 * 1000));
                        return new Date(datePlus24.getFullYear(), datePlus24.getMonth(), datePlus24.getDate(), 6, 0, 0, 0);
                    }
                    else { return null; }
                })
            };
            this.DetailView = ko.observable(false);
            this.crossDetail = new ObservableCross();
            this.PreviewMode = ko.observable(previewMode);
            this.ReturnToListView = function () {
                self.DetailView(false);
            };
            app.prepareAjax(false);

            Sammy(function () {
                this.get('#crosses', function () {
                    if (canViewAll) {
                        self.crossService.getCrossList((data: Cross[]): void => {
                                self.dataModel.Crosses.removeAll();
                                $.each(data, function (i, d) {
                                    var oCross = <ObservableCross><any>ko.mapping.fromJS(d, KnockoutMapperConfigurations.koCrossMapperConfiguration, <KnockoutObservableType<Cross>><any>new ObservableCross());
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
                        self.crossService.getPublicCross((data: Cross) => {
                                self.crossDetail.questions.removeAll();
                                ko.mapping.fromJS(data, KnockoutMapperConfigurations.koCrossMapperConfiguration, <KnockoutObservableType<Cross>><any>self.crossDetail);
                                if (data != null) {
                                    self.DetailView(true);
                                }
                            });
                    }
                });
                this.post('', function () { return true; });        //for image upload
                this.get('/crosses', function () { this.app.runRoute('get', '#crosses'); });
                this.get('/Crosses', function () { this.app.runRoute('get', '#crosses'); });
                this.get('/Crosses#detail', function () { this.app.runRoute('get', '/crosses#detail', this.params); });
                this.get('/crosses#detail', function () {
                    var currContext = this as Sammy.EventContext;
                    self.crossId(currContext.params.id);
                    if (self.dataModel.Crosses().length === 0) {
                        self.crossService.getCrossDetail(currContext.params.id, (data: Cross) => {
                                self.crossDetail.questions.removeAll();
                                self.crossDetail.days.removeAll();
                                ko.mapping.fromJS(data, KnockoutMapperConfigurations.koCrossMapperConfiguration, <KnockoutObservableType<Cross>><any>self.crossDetail);
                                self.retrieveDays(currContext.params.id);
                                self.retrieveCrossQuestions();
                            });
                    }
                    else {
                        self.crossDetail.questions.removeAll();
                        $.each(self.dataModel.Crosses(), function (i, e: ObservableCross) {
                            if (e.id() === currContext.params.id) {
                                var crossToEdit = ko.mapping.toJS(e);
                                ko.mapping.fromJS(crossToEdit, KnockoutMapperConfigurations.koCrossMapperConfiguration, self.crossDetail);
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

        retrieveDays = (crossId: string): void => {
            var self = this;
            this.crossService.getCrossDays(crossId,
                (data: CrossDay[]) => {
                    self.crossDetail.days.removeAll();
                    $.each(data, (i, day) => {
                        var oDay = new ObservableCrossDay();
                        oDay.passage(day.passage);
                        oDay.weight(day.weight);
                        oDay.isCheckpoint(day.isCheckpoint);
                        oDay.id = day.id;
                        oDay.index(day.index);
                        $.each(day.questions, (j, q) => {
                            var oq = new ObservableCrossQuestion();
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
                }
            );
        }

        retrieveDayQuestions = (): void => {
            var self = this;
            this.crossService.retrieveTemplateQuestions((data: CrossQuestion[]) => {
                if (self.crossDetail.days().length > 0) {
                    for (let i = 0; i < self.crossDetail.days().length; i++) {
                        self.crossDetail.days()[i].questions.removeAll();
                        ko.mapping.fromJS(data, <KnockoutMappingOptions<CrossQuestion[]>>{
                            create: function (options: KnockoutMappingCreateOptions) {
                                return self.CreateObservableQuestion(self.crossDetail, options.data);
                            }
                        }, <KnockoutReadonlyObservableArrayType<CrossQuestion>><any>self.crossDetail.days()[i].questions);
                    }
                }
            });
        }

        retrieveCrossQuestions = (): void => {
            var self = this;
            this.crossService.retrieveCrossQuestions(self.crossDetail.id(), (data: CrossQuestion[]) => {
                self.crossDetail.questions.removeAll();
                ko.mapping.fromJS(data, <KnockoutMappingOptions<CrossQuestion[]>>{
                    create: function (options: KnockoutMappingCreateOptions) {
                        return self.CreateObservableQuestion(self.crossDetail, options.data);
                    }
                }, <KnockoutReadonlyObservableArrayType<CrossQuestion>><any>self.crossDetail.questions);
            });
        }

        GetPendingApprovals = (): void => {
            var self = this;
            this.crossService.getPendingApprovals(self.crossDetail.id(),
                (data: PendingCrossApproval[]) => {
                    self.dataModel.PendingCrossApprovals(data);
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Retrieve Failure!",
                        message: "A problem has been occurred attempting to retrieve the pending cross approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        retrievePinnedCrosses = (): void => {
            var self = this;
            this.crossService.getPinnedCrosses((data: PinnedCross[]) => {
                    $.each(data, (i, val) => {
                        $.each(self.dataModel.Crosses(), (j, cross: ObservableCross) => {
                            if (cross.id() === val.crossId) {
                                cross.isPinned(true);
                                return false;
                            }
                        });
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Pins Retrieval Failed!",
                        message: "Could not retrieve pins"
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                }
            );
        }

        retrieveCompletedCrosses = (): void => {
            var self = this;
            this.crossService.getCompletedCrosses((data: Cross[]) => {
                    $.each(data, (i, val: Cross) => {
                        $.each(self.dataModel.Crosses(), (j, cross: ObservableCross) => {
                            if (cross.id() === val.id) {
                                cross.isCompleted(true);
                                return false;
                            }
                        });
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Completed Ring Retrieval Failed!",
                        message: "Could not retrieve completed crosses"
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                }
            );
        }

        saveNewCrossOrder = (data): void => {
            var self = this;
            var crossOrder = [];
            $.each(self.dataModel.Crosses(), function (index, value) {
                var objToSave = { Id: value.id(), Index: index + 1 };
                crossOrder.push(objToSave);
            });
            this.crossService.updateCrossOrder(crossOrder,(data: Cross) => {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "Cross Order has been updated"
                    });
                },
                (err: JQueryXHR) => {
                    data.cancelDrop = true;
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "Cross order could not be updated"
                    });
                }
            );
        };

        saveCross = (): void => {
            var self = this;
            this.crossService.createCross({ Name: this.dataModel.Name(), Description: this.dataModel.Description() },
                (data: Cross) => {
                    var oCross = <ObservableCross><any>ko.mapping.fromJS(data, KnockoutMapperConfigurations.koCrossMapperConfiguration, <KnockoutObservableType<Cross>><any>new ObservableCross());
                    self.dataModel.Crosses.push(oCross);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The Cross has been saved"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The Cross save process failed"
                    });
                }
            );
        }

        updateCross = (data: ObservableCross): void => {
            var self = this;

            this.crossService.updateCross({ Id: data.id(), Name: data.name(), Description: data.description(), ExplainText: data.explainText() },
                (data: Cross) => {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The Cross has been saved"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The Cross could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        saveCrossDaysAndQuestions = (data: ObservableCross): void => {
            this.updateCross(data);
            this.saveDays(data);
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

        saveDays = (data: ObservableCross): void => {
            var self = this;
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            var daysToSave = [];
            $.each(data.days(), function (index, value: ObservableCrossDay) {
                var objToSave = { Passage: value.passage(), Weight: value.weight(), isCheckpoint: value.isCheckpoint(), index: index, id: value.id };
                if (value.id !== '') objToSave['DayId'] = value.id;
                daysToSave.push(objToSave);
            });
            this.crossService.updatePassages(data.id(),
                daysToSave,
                (data: CrossDay[]) => {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The days have been saved"
                    });
                    self.crossDetail.days.removeAll();
                    $.each(data, (i, day) => {
                        var oDay = new ObservableCrossDay();
                        oDay.passage(day.passage);
                        oDay.weight(day.weight);
                        oDay.isCheckpoint(day.isCheckpoint);
                        oDay.id = day.id;
                        self.crossDetail.days.push(oDay);
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The days could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
        }

        saveDayAnswers = (data: ObservableCrossDay): void => {
            var self = this;
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            var answersToSave = [];
            $.each(data.questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            this.crossService.savePassageAnswers(self.crossDetail.id(), data.id, answersToSave,
                (data: Cross) => {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "Your answers have been saved"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "Your answers could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        saveAnswers = (data: ObservableCross): void => {
            var self = this;
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            var answersToSave = [];
            $.each(data.questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            this.crossService.saveSummaryAnswers(data.id(), answersToSave,
                (data: Cross) => {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "Your answers have been saved"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "Your answers could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        completeDay = (data: ObservableCrossDay): void => {
            var self = this;
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            var answersToSave = [];
            $.each(data.questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            this.crossService.completeCrossDay(self.crossDetail.id(),
                data.id,
                answersToSave,
                () => {
                    data.completedAt(new Date());
                    if (data.isCheckpoint()) {
                        var pendingApproval = new PendingCrossApproval();
                        pendingApproval.dayId = data.id;
                        self.dataModel.PendingCrossApprovals.push(pendingApproval);
                    }
                    data.editing(false);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The day has been completed"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The day could not be completed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        returnDay = (data: ObservableCrossDay): void => {
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
        }

        completeCross = (data: ObservableCross): void => {
            var self = this;
            if (data.warriorCompleted())
                return;
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            var answersToSave = [];
            $.each(data.questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossId: data.id(), CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            this.crossService.completeCross(data.id(),
                answersToSave,
                (result) => {
                    data.completedAt(new Date());
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been completed"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be completed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        returnCross = (data: ObservableCross): void => {
            var self = this;
            if (!data.warriorCompleted())
                return;
            this.crossService.returnCross(data.id(),
                () => {
                    data.completedAt(null);
                    self.dataModel.PendingCrossApprovals.removeAll();
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been returned"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        confirmCrossComplete = (data: ObservableCross): void => {
            var self = this;
            this.crossService.confirmCrossComplete(self.dataModel.PendingCrossApprovals()[0].approvalRecordId,
                () => {
                    self.dataModel.PendingCrossApprovals.removeAll();
                    self.crossDetail.approvedAt(new Date());
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been confirmed"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be confirmed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        confirmCrossDayComplete = (data: ObservableCrossDay): void => {
            var self = this;
            this.crossService.confirmCrossDayComplete(self.dataModel.PendingCrossApprovals()[0].approvalRecordId,
                () => {
                    self.dataModel.PendingCrossApprovals.removeAll();
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been confirmed"
                    });
                },
                (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be confirmed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            )
        }

        pin = (data: ObservableCross): void => {
            var self = this;
            this.crossService.pinCross(data.id(),
                (ring: Cross) => {
                    data.isPinned(true);
                    BootstrapAlert.success({
                        title: "Pin Success!",
                        message: "The cross has been pinned"
                    });
                },
                (err) => {
                    BootstrapAlert.alert({
                        title: "Pin Failed!",
                        message: "Could not pin the cross"
                    });
                }
            );
        }

        unpin = (data: ObservableCross): void => {
            var self = this;
            this.crossService.unpinCross(data.id(),
                (ring: Cross) => {
                    data.isPinned(false);
                    BootstrapAlert.success({
                        title: "Unpin Success!",
                        message: "The cross has been un-pinned"
                    });
                },
                (err) => {
                    BootstrapAlert.alert({
                        title: "Unpin Failed!",
                        message: "Could not un-pin the cross"
                    });
                }
            );
        }

        CreateObservableQuestion = (cross: ObservableCross, data: CrossQuestion): ObservableCrossQuestion => {
            var self = this;
            var result = new ObservableCrossQuestion();
            result.id = data.id;
            result.crossId = cross.id();
            var formattedText = data.text.replace('{BookName}', cross.name());
            if (userIsWarrior || previewMode) {
                formattedText = formattedText.replace('{explain}', cross.explainText())
            }
            result.text = formattedText;
            result.answer(data.answer);
            return result;
        }

        isAnswerTextAreaEnabled = (userIsWarrior: boolean, day: ObservableCrossDay) => {
            return userIsWarrior
                && ((day === this.dataModel.nextDayToComplete()
                    && new Date() > this.dataModel.timeToUnlock())
                    || (day.editing() && day.warriorCompleted()))
                && (this.dataModel.PendingCrossApprovals().length === 0 || day.index() < this.dataModel.nextDayToComplete().index());
        }

        summaryQuestionsEnabled = (userIsWarrior: boolean) => {
            return userIsWarrior && this.dataModel.PendingCrossApprovals().length === 0 && !this.crossDetail.days().some(d => d.completedAt() === null);
        }

        summaryQuestionsVisible = (userIsWarrior: boolean) => {
            return (userIsWarrior && !this.crossDetail.days().some(d => d.completedAt() === null) && this.dataModel.PendingCrossApprovals().length === 0)
                || (!userIsWarrior && this.crossDetail.warriorCompleted());
        }
    }
}

WarriorsGuild.app.addViewModel({
    name: "Crosses",
    bindingMemberName: "crosses",
    factory: WarriorsGuild.CrossesViewModel,
    allowUnauthorized: true
});