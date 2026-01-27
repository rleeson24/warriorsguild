var WarriorsGuild;
(function (WarriorsGuild) {
    var CrossDayQuestionViewModel = /** @class */ (function () {
        function CrossDayQuestionViewModel(params) {
            this.cross = params.cross;
            this.crossDay = params.crossDay;
            this.question = params.question;
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.showEntryField = params.showEntryField;
            this.enableEntryField = params.enableEntryField;
        }
        return CrossDayQuestionViewModel;
    }());
    WarriorsGuild.CrossDayQuestionViewModel = CrossDayQuestionViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('cross-day-question-answer', {
    viewModel: WarriorsGuild.CrossDayQuestionViewModel,
    template: "<div class=\"col-xs-12 text-left\">\n\t                <p data-bind=\"text: question.text\"></p>\n                    <span data-bind=\"if: showEntryField()\">\n\t\t                <textarea class=\"form-control\" data-bind=\"value: question.answer, enabled: enableEntryField\"></textarea>\n                    </span>\n                    <span data-bind=\"if: !showEntryField()\">\n\t\t                <p class=\"answer\" data-bind=\"text: question.answer()\"></p>\n                    </span>\n                </div>"
});
//# sourceMappingURL=crossDayQuestion.component.js.map