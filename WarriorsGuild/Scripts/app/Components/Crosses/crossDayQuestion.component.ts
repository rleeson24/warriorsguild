namespace WarriorsGuild {
    export class CrossDayQuestionViewModel {
        cross: Cross;
        crossDay: ObservableCrossDay;
        question: ObservableCrossQuestion;
        hasActiveSubscription: boolean;
        userIsWarrior: boolean;
        showEntryField: KnockoutObservable<boolean>;
        enableEntryField: KnockoutObservable<boolean>;

        constructor(params) {
            this.cross = params.cross;
            this.crossDay = params.crossDay;
            this.question = params.question;
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.showEntryField = params.showEntryField;
            this.enableEntryField = params.enableEntryField;
        }
    }
}

ko.components.register('cross-day-question-answer', {
    viewModel: WarriorsGuild.CrossDayQuestionViewModel,
    template: `<div class="col-xs-12 text-left">
	                <p data-bind="text: question.text"></p>
                    <span data-bind="if: showEntryField()">
		                <textarea class="form-control" data-bind="value: question.answer, enabled: enableEntryField"></textarea>
                    </span>
                    <span data-bind="if: !showEntryField()">
		                <p class="answer" data-bind="text: question.answer()"></p>
                    </span>
                </div>`
});