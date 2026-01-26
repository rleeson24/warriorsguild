namespace WarriorsGuild {
    export class CrossDayViewModel {
        cross: KnockoutObservable<WarriorsGuild.ObservableCross>;
        crossDay: WarriorsGuild.ObservableCrossDay;
        hasActiveSubscription: boolean;
        userIsWarrior: boolean;
        baseUrl: string = '/api/crossStatus';
        currentDay: KnockoutComputed<ObservableCrossDay | null>
        lastDayCompleted: KnockoutComputed<ObservableCrossDay | null>
        nextDayToComplete: KnockoutComputed<ObservableCrossDay | null>
        pendingApproval: KnockoutObservable<PendingCrossApproval>;
        answersSavedCallback: Function;
        isCollapsed: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        showEntryField: KnockoutComputed<boolean>
        enableEntryField: KnockoutComputed<boolean>

        constructor(params) {
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.
            
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
            this.showEntryField = ko.computed(() => {
                return this.crossDay && (!this.crossDay.warriorCompleted() || this.crossDay.editing()) && this.hasActiveSubscription
            });
            this.enableEntryField = ko.computed(() => {
                return this.crossDay && this.crossDay.editing() && userIsWarrior;
            });
        }

        timeToUnlock: KnockoutComputed<Date | null> = ko.pureComputed<Date | null>(() => {
            return new Date(2000, 1, 1);
            let d = this.lastDayCompleted()?.completedAt();
            if (d) {
                var datePlus24 = new Date(d.getTime() + (24 * 60 * 60 * 1000));
                return new Date(datePlus24.getFullYear(), datePlus24.getMonth(), datePlus24.getDate(), 6, 0, 0, 0);
            }
            else { return null; }
        })

        isAnswerTextAreaEnabled: KnockoutObservable<boolean> = ko.pureComputed<boolean>(() => {
            return this.userIsWarrior
                && (this.crossDay === this.nextDayToComplete() || this.crossDay.editing())
                && (new Date() > this.timeToUnlock())
                && (!this.pendingApproval() || this.crossDay.index() < this.nextDayToComplete().index());
        })

        visible: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => {
            return !this.nextDayToComplete()
                || (this.crossDay.index() < this.nextDayToComplete().index())
                || (this.crossDay === this.nextDayToComplete() && this.pendingApproval() !== null)
                ;
        })

        showComplete: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => {
            return (this.userIsWarrior && this.crossDay.warriorCompleted()) || this.crossDay.guardianReviewed()
        })

        saveDayAnswers = (): void => {
            var self = this;
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            var answersToSave = [];
            $.each(this.crossDay.questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            WarriorsGuild.serviceBase.put({
                url: this.baseUrl + `/${self.cross().id()}/day/${this.crossDay.id}/answers`,
                data: ko.toJSON(answersToSave),
                contentType: "application/json; charset=utf-8",
                success: function (data: Cross) {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "Your answers have been saved"
                    });
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "Your answers could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            });
        }

        completeDay = (): void => {
            var self = this;
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            var answersToSave = [];
            $.each(this.crossDay.questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            WarriorsGuild.serviceBase.post({
                url: `${this.baseUrl}/${self.cross().id()}/day/${this.crossDay.id}/complete`,
                data: ko.toJSON(answersToSave),
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                    self.crossDay.completedAt(new Date());
                    if (self.crossDay.isCheckpoint()) {
                        var pendingApproval = new PendingCrossApproval();
                        pendingApproval.dayId = self.crossDay.id;
                    }
                    self.answersSavedCallback(pendingApproval);
                    self.crossDay.editing(false);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The day has been completed"
                    });
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The day could not be completed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            });
        }

        returnDay = (data: ObservableCrossDay): void => {
            this.crossDay.editing(true);
        }

        toggleCollapsed = (): void => {
            this.isCollapsed(!this.isCollapsed());
        }
    }
}

ko.components.register('cross-day', {
    viewModel: WarriorsGuild.CrossDayViewModel,
    template: `
                <div class="panel panel-default" data-bind="if: crossDay && visible()">
                    <div class="panel-heading">
                        <h2 class="panel-title" data-toggle="collapse" data-bind="click: toggleCollapsed">Passage <span data-bind="text: crossDay.index() + 1"></span><span data-bind="visible: showComplete()"> (Complete)</span></h2>
                    </div>
                    <div class="panel-collapse" data-bind="css: { collapse: isCollapsed() }">
                        <div class="panel-body">
                            <!-- ko if: timeToUnlock() > new Date() -->
						        <!-- ko if: userIsWarrior -->
						            Available 6 AM tomorrow
						        <!-- /ko -->
						    <!-- /ko -->
                            <!-- ko if: timeToUnlock() < new Date() -->
                                <!-- ko if: userIsWarrior || crossDay.warriorCompleted() -->
                                    <h4>Pray for focus and wisdom</h4>
				                    <h4>Read <span data-bind="text: crossDay.passage()"></span></h4>
				                    <div class="row" data-bind="foreach: crossDay.questions">
					                    <cross-day-question-answer params="{cross: $component.cross,
                                                                            crossDay: $component.crossDay,
                                                                            question: $data,
                                                                            hasActiveSubscription: $component.hasActiveSubscription,
                                                                            userIsWarrior: $component.userIsWarrior,
                                                                            showEntryField: $component.showEntryField
                                                                            enableEntryField: $component.enableEntryField }" />
				                    </div>

				                    <div class="row" style="margin-top: 20px">
					                    <div class="col-xs-12">
						                    <button class="btn btn-sm btn-primary" data-bind="click: function(data) { crossDay.editing(false); }, visible: crossDay.editing()">Cancel Edit</button>
						                    <button class="btn btn-sm btn-primary btn-save-answers" data-bind="click: completeDay, visible: isAnswerTextAreaEnabled()">Save Answers</button>
						                    <button class="btn btn-sm btn-danger" data-bind="click: returnDay, visible: userIsWarrior && crossDay.warriorCompleted() && !crossDay.editing()">Edit Answers</button>
                                            <div data-bind="visible: userIsWarrior && crossDay.warriorCompleted() && timeToUnlock() > new Date() && crossDay === lastDayCompleted()">
								                <p>You have completed Day <span data-bind="text: $index() + 1"></span>.</p>
						                    </div>
					                    </div>
				                    </div>
						        <!-- /ko -->
                                <!-- ko if: !userIsWarrior && !crossDay.warriorCompleted() -->
								    <p>Awaiting Warrior Completion</p>                                
						        <!-- /ko -->
						    <!-- /ko -->
                        </div>
                    </div>
                </div>
`
});