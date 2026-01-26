declare var crossSummaryQuestionsWeight: number;

namespace WarriorsGuild {
    export class CrossSummaryQuestionsViewModel {
        cross: KnockoutObservable<ObservableCross>;
        questions: KnockoutObservableArray<ObservableCrossQuestion>;
        crossService: CrossService;
        hasActiveSubscription: boolean;
        userIsWarrior: boolean;
        pendingApproval: KnockoutObservable<PendingCrossApproval>;
        answersSavedCallback: Function;
        isCollapsed: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        showEntryField: KnockoutComputed<boolean>
        enableEntryField: KnockoutComputed<boolean>
        
        constructor(params) {
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.

            this.crossService = new CrossService();
            this.cross = params.cross;
            this.questions = params.questions;
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.pendingApproval = params.pendingApproval;
            this.answersSavedCallback = params.answersSavedCallback;
            this.enableEntryField = params.questionsEnabled;
            this.showEntryField = params.showSummaryQuestionsEntryField;
        }

        saveAnswers = (): void => {
            let self = this;
            var answersToSave = [];
            $.each(this.questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            this.crossService.saveSummaryAnswers(this.cross().id(),
                answersToSave,
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
                });
        }

        completeCross = (): void => {
            var self = this;
            if (this.cross().warriorCompleted())
                return;
            var answersToSave = [];
            $.each(this.cross().questions(), function (index, value: ObservableCrossQuestion) {
                var objToSave = { CrossId: self.cross().id(), CrossQuestionId: value.id, Answer: value.answer() };
                if (value.id !== '') objToSave['Id'] = value.id;
                answersToSave.push(objToSave);
            });
            this.crossService.completeCross(this.cross().id(),
                answersToSave,
                (result: PendingCrossApproval) => {
                    self.cross().completedAt(new Date());
                    self.answersSavedCallback(result);
                    self.pendingApproval(result);
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
                });
        }

        confirmCrossComplete = (): void => {
            var self = this;
            this.crossService.confirmCrossComplete(self.pendingApproval().approvalRecordId,
                () => {
                    self.pendingApproval(null);
                    self.cross().approvedAt(new Date());
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
                });
        }

        returnCross = (): void => {
            var self = this;
            if (!this.cross().warriorCompleted())
                return;
            this.crossService.returnCross(this.cross().id(),
                () => {
                    self.cross().completedAt(null);
                    self.answersSavedCallback(100 - crossSummaryQuestionsWeight);
                    self.pendingApproval(null);
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
                });
        }
    }
}

ko.components.register('cross-summary-questions', {
    viewModel: WarriorsGuild.CrossSummaryQuestionsViewModel,
    template: `
				<div class="row"><h3>Summary</h3></div>
				<div class="row" data-bind="foreach: cross().questions">
					<cross-day-question-answer params="{cross: $component.cross,
                                                                    question: $data,
                                                                    hasActiveSubscription: $component.hasActiveSubscription,
                                                                    userIsWarrior: $component.userIsWarrior,
                                                                    showEntryField: $component.showEntryField
                                                                    }" />
				</div>

                <!-- ko if: hasActiveSubscription -->
                    <!-- ko if: userIsWarrior -->
					    <div class="row" style="margin-top: 20px">
							<div class="col-xs-12">
								<button class="btn btn-sm btn-primary" data-bind="click: saveAnswers, visible: !cross().warriorCompleted() && $component.showEntryField()">Save Answers</button>
								<button class="btn btn-sm btn-success" data-bind="click: completeCross, visible: !cross().warriorCompleted() && $component.showEntryField()">Submit for Review</button>
								<button class="btn btn-sm btn-danger" data-bind="click: returnCross, visible: cross().warriorCompleted() && !cross().guardianReviewed()">Recall Request for Review</button>
							</div>
						</div>
                    <!-- /ko -->
                    <!-- ko if: userIsGuardian -->
					    <div class="row" style="margin-top: 20px">
							<div class="col-xs-12">
								<button class="btn btn-sm btn-primary" data-bind="click: confirmCrossComplete, visible: cross().warriorCompleted() && !cross().guardianReviewed()">Confirm Completion</button>
								<button class="btn btn-sm btn-danger" data-bind="click: returnCross, visible: cross().warriorCompleted() && !cross().guardianReviewed()">Return to Warrior</button>
							</div>
						</div>
					<!-- /ko -->
                    <!-- ko if: cross().guardianReviewed() -->
					<div class="row">
						<div class="col-xs-12">
							<h3>Completed and Confirmed!</h3>
						</div>
					</div>
					<!-- /ko -->
                <!-- /ko -->
            `
});
