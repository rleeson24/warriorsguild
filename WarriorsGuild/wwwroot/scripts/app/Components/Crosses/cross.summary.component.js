var WarriorsGuild;
(function (WarriorsGuild) {
    var CrossSummaryViewModel = /** @class */ (function () {
        function CrossSummaryViewModel(params) {
            var _this = this;
            this.cross = ko.observable(new WarriorsGuild.ObservableCross());
            this.days = ko.observableArray([]);
            this.daysCompleteMessage = ko.pureComputed(function () {
                return "".concat(_this.days().filter(function (d) { return d.warriorCompleted(); }).length, " of ").concat(_this.days().length, " days completed");
            });
            this.percentWeightPerCheckpoint = ko.pureComputed(function () {
                var result = 0;
                if (_this.days() !== null) {
                    result = _this.days().filter(function (d) { return d.isCheckpoint(); }).length;
                }
                return result;
            });
            this.allDaysComplete = function () {
                return !_this.days().some(function (d) { return d.completedAt() === null; });
            };
            this.getCrossDetail = function (id) {
                var self = _this;
                _this.crossService.getCrossDetail(id, function (data) {
                    ko.mapping.fromJS(data, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, self.cross());
                    self.getCrossDays();
                });
            };
            this.getCrossDays = function () {
                var self = _this;
                self.days.removeAll();
                _this.crossService.getCrossDays(_this.cross().id(), function (data) {
                    $.each(data, function (i, day) {
                        var oDay = new WarriorsGuild.ObservableCrossDay();
                        oDay.approvedAt(WarriorsGuild.DateConversion.convertStringToNullableDate(day.approvedAt));
                        oDay.completedAt(WarriorsGuild.DateConversion.convertStringToNullableDate(day.completedAt));
                        self.days.push(oDay);
                    });
                });
            };
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.
            this.crossId = params.crossId;
            this.crossService = new WarriorsGuild.CrossService();
            this.getCrossDetail(this.crossId());
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.userIsGuardian = params.userIsGuardian;
            this.crossId.subscribe(function (newValue) {
                _this.getCrossDetail(newValue);
            });
            this.warriorCompleted = ko.pureComputed(function () {
                return _this.cross().warriorCompleted();
            });
            this.name = ko.pureComputed(function () {
                return _this.cross().name();
            });
            this.imgSrcAttr = ko.pureComputed(function () {
                return _this.cross().imgSrcAttr();
            });
        }
        return CrossSummaryViewModel;
    }());
    WarriorsGuild.CrossSummaryViewModel = CrossSummaryViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('cross-summary', {
    viewModel: WarriorsGuild.CrossSummaryViewModel,
    template: "\n                <h3><img style=\"height:40px\" data-bind=\"attr: { src: imgSrcAttr, alt: name, title: name }\" /><span data-bind=\"text: name\"></span></h3>\n                <h3 data-bind=\"if: allDaysComplete() && warriorCompleted()\">Complete</h3>\n                <div data-bind=\"if: !warriorCompleted()\">\n                    <h3 data-bind=\"text: daysCompleteMessage()\"></h3>\n                </div>\n                <h3 data-bind=\"if: allDaysComplete() && !warriorCompleted()\">Summary Pending</h3>\n            "
});
//# sourceMappingURL=cross.summary.component.js.map