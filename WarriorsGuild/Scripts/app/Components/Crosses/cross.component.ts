declare var crossSummaryQuestionsWeight: number;

namespace WarriorsGuild {
	export class CrossViewModel {
		cross: KnockoutObservable<WarriorsGuild.ObservableCross> = ko.observable<WarriorsGuild.ObservableCross>(new ObservableCross());
		hasActiveSubscription: boolean;
		userIsWarrior: boolean;
		userIsGuardian: boolean;
		days: KnockoutObservableArray<ObservableCrossDay> = ko.observableArray([]);
		PendingApproval: KnockoutObservable<PendingCrossApproval> = ko.observable<PendingCrossApproval>();
		crossService: CrossService;
		totalCrossWeight: number;
		crossId: KnockoutObservable<string>;
		completionPercentUpdatedHandler: Function;
		showAllDaysSinceCheckpoint: boolean;

		constructor(params) {
			// 'params' is an object whose key/value pairs are the parameters
			// passed from the component binding or custom element.
			this.crossId = params.crossId;
			this.crossService = new CrossService();
			this.getCrossDetail(this.crossId())
			this.hasActiveSubscription = params.hasActiveSubscription;
			this.userIsWarrior = params.userIsWarrior;
			this.userIsGuardian = params.userIsGuardian;
			this.totalCrossWeight = params.totalCrossWeight;
			this.completionPercentUpdatedHandler = params.completionPercentUpdatedHandler;
			this.showAllDaysSinceCheckpoint = params.showAllDaysSinceCheckpoint;

			this.crossId.subscribe(newValue => {
				this.getCrossDetail(newValue)
			});
		}

		private totalPercentComplete: KnockoutComputed<number> = ko.pureComputed<number>(() => {
			return this.numberOfCompletedDays() * this.percentWeightPerDay();
		});

		private numberOfDays: KnockoutComputed<number> = ko.pureComputed<number>(() => {
			return this.days().length;
		});

		private numberOfCompletedDays: KnockoutComputed<number> = ko.pureComputed<number>(() => {
			return this.days().filter(d => !!d.completedAt()).length;
		});

		percentWeightPerDay: KnockoutComputed<number> = ko.pureComputed<number>(() => {
			let result = 0;
			if (this.days() !== null) {
				result = (100 - crossSummaryQuestionsWeight) / this.numberOfDays();
			}
			return result;
		})

		lastDayCompleted: KnockoutComputed<ObservableCrossDay | null> = ko.pureComputed<ObservableCrossDay | null>(() => {
			if (this.days() === null) {
				return null;
			}
			let lastCompleted = null;
			for (let d in this.days()) {
				let curr = this.days()[d];
				if (curr.warriorCompleted()) {
					lastCompleted = curr;
				}
				else {
					break;
				}
			}
			return lastCompleted;
		})

		nextDayToComplete: KnockoutComputed<ObservableCrossDay | null> = ko.pureComputed<ObservableCrossDay | null>(() => {
			if (this.days() === null) {
				return null;
			}
			let firstIncomplete = null;
			for (let d in this.days()) {
				let curr = this.days()[d];
				if (!curr.warriorCompleted()) {
					firstIncomplete = curr;
					break;
				}
			}
			return firstIncomplete;
		})
		MAX_DAYS_TO_SHOW = 3;
		daysToShow: KnockoutComputed<ObservableCrossDay[]> = ko.pureComputed<ObservableCrossDay[]>(() => {
			var currentDayIndex = this.days().indexOf(this.currentDay());
			var daysToShow = currentDayIndex > -1 ? 1 : 0;
			var previousDays = this.days().slice(0, currentDayIndex);
			for (var i = previousDays.length - 1; i >= 0; i--) {
				var item = previousDays[i];
				if (!item.isCheckpoint()) daysToShow++;
				else {
					break;
                }
				if (!this.showAllDaysSinceCheckpoint && daysToShow === this.MAX_DAYS_TO_SHOW) {
					break;
				}
			}
			var startPosition = Math.max(currentDayIndex - (daysToShow - 1), 0);
			return this.days().slice(startPosition, startPosition + daysToShow);
		});

		currentDay: KnockoutComputed<ObservableCrossDay> = ko.computed<ObservableCrossDay>(() => {
			let result = null;
			if (this.PendingApproval()) {
				result = this.lastDayCompleted();
			}
			else {
				result = this.nextDayToComplete();
			}
			return result;
		})

		allDaysComplete = () => {
			return !this.days().some(d => d.completedAt() === null)
		}

		summaryQuestionsEnabled = () => {
			return this.summaryQuestionsVisible() && (this.userIsWarrior || (!this.userIsWarrior && this.cross().warriorCompleted()));
		}

		summaryQuestionsVisible = () => {
			return (!this.PendingApproval() || this.PendingApproval().dayId === null) && this.allDaysComplete();
		}

		showSummaryQuestionsEntryField = () => {
			return this.userIsWarrior && this.summaryQuestionsVisible() && !this.PendingApproval();
		}

		getCrossDetail = (id: string) => {
			let self = this;
			this.crossService.getCrossDetail(id,
				function (data: Cross) {
					ko.mapping.fromJS(data, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, <KnockoutObservableType<Cross>><any>self.cross());
					self.retrieveCrossQuestions();
					self.getCrossDays();
					self.GetPendingApprovals();
				});
		}

		retrieveCrossQuestions = (): void => {
			let self = this;
			this.crossService.retrieveCrossQuestions(self.cross().id(),
				(data: CrossQuestion[]) => {
					self.cross().questions.removeAll();
					ko.mapping.fromJS(data, <KnockoutMappingOptions<CrossQuestion[]>>{
						create: function (options: KnockoutMappingCreateOptions) {
							return self.CreateObservableQuestion(options.data);
						}
					}, <KnockoutReadonlyObservableArrayType<CrossQuestion>><any>self.cross().questions);
				});
		}

		getCrossDays = (): void => {
			var self = this;
			self.days.removeAll();
			this.crossService.getCrossDays(this.cross().id(),
				(data: CrossDay[]) => {
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
						self.days.push(oDay);
					});
					self.notifyParentOfPercentCompleteUpdated();
				});
		}

		answersSavedCallback = (data: PendingCrossApproval): void => {
			var self = this;
			self.PendingApproval(data);
			self.notifyParentOfPercentCompleteUpdated();
		}

		notifyParentOfPercentCompleteUpdated = (): void => {
			debounce(() => {
				let self = this;
				if (self.completionPercentUpdatedHandler) {
					if (!!self.PendingApproval() && self.PendingApproval().percentComplete > 0) {
						self.completionPercentUpdatedHandler(self.PendingApproval().percentComplete);
					}
					else {
						self.completionPercentUpdatedHandler(this.totalPercentComplete());
					}
				}
			},300)();
		}

		GetPendingApprovals = (): void => {
			var self = this;
			this.crossService.getPendingApprovals(this.cross().id(),
				(data: PendingCrossApproval[]) => {
					if (data.length > 0) {
						self.PendingApproval(data[0]);
						self.notifyParentOfPercentCompleteUpdated();
					}
				},
				(err: JQueryXHR) => {
					BootstrapAlert.alert({
						title: "Retrieve Failure!",
						message: "A problem has been occurred attempting to retrieve the pending cross approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				});
		}

		saveAnswers = (): void => {
			var self = this;
			var answersToSave = [];
			$.each(this.cross().questions(), function (index, value: ObservableCrossQuestion) {
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
					self.completionPercentUpdatedHandler(this.percentWeightPerDay() * this.numberOfCompletedDays());
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
					self.PendingApproval(result);
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
			this.crossService.confirmCrossComplete(self.PendingApproval().approvalRecordId,
				() => {
					self.PendingApproval(null);
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

		confirmCrossCheckpointComplete = (): void => {
			var self = this;
			this.crossService.confirmCrossCheckpointComplete(self.PendingApproval().approvalRecordId,
				() => {
					self.PendingApproval(null);
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
					self.PendingApproval(null);
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

		CreateObservableQuestion = (data: CrossQuestion): ObservableCrossQuestion => {
			let self = this;
			var result = new ObservableCrossQuestion();
			result.id = data.id;
			result.crossId = self.cross().id();
			var formattedText = data.text.replace('{BookName}', self.cross().name());
			if (userIsWarrior || previewMode) {
				formattedText = formattedText.replace('{explain}', self.cross().explainText())
			}
			result.text = formattedText;
			result.answer(data.answer);
			return result;
		}
	}
}

ko.components.register('cross', {
	viewModel: WarriorsGuild.CrossViewModel,
	template: `
				<div data-bind="foreach: daysToShow()" class="panel-group">
					<cross-day params="{
											cross: ko.unwrap($component.cross()),
											crossDay: $data,
											currentDay: $component.currentDay(),
											lastDayCompleted: $component.lastDayCompleted(),
											nextDayToComplete: $component.nextDayToComplete(),
											hasActiveSubscription: $component.hasActiveSubscription,
											userIsWarrior: $component.userIsWarrior,
											pendingApproval: $component.PendingApproval,
											answersSavedCallback: $component.answersSavedCallback
										}" />
				</div>
				<div class="row" data-bind="if: !!PendingApproval()?.dayId">
					<h4 data-bind="if: userIsWarrior">You are progressing well! Time for a fireside chat with your guardian.</h4>
					<div class="col-xs-12" data-bind="if: userIsGuardian">
						<button class="btn btn-sm btn-primary" data-bind="click: confirmCrossCheckpointComplete">Fireside Chat Complete</button>
					</div>
				</div>
				<!-- ko if: summaryQuestionsVisible() -->
					<cross-summary-questions params="{
														hasActiveSubscription: $component.hasActiveSubscription,
														userIsWarrior: $component.userIsWarrior,
														userIsGuardian: $component.userIsGuardian,
														cross: $component.cross,
														questions: $component.cross().questions,
														pendingApproval: $component.PendingApproval,
														answersSavedCallback: $component.answersSavedCallback,
														questionsEnabled: $component.summaryQuestionsEnabled,
														showSummaryQuestionsEntryField: $component.showSummaryQuestionsEntryField
														}"></cross-summary-questions>
				<!-- /ko -->
			`
});
