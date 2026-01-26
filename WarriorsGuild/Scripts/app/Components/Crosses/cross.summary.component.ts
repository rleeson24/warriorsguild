namespace WarriorsGuild {
    export class CrossSummaryViewModel {
        cross: KnockoutObservable<WarriorsGuild.ObservableCross> = ko.observable<WarriorsGuild.ObservableCross>(new ObservableCross());
        hasActiveSubscription: boolean;
        userIsWarrior: boolean;
        userIsGuardian: boolean;
        days: KnockoutObservableArray<ObservableCrossDay> = ko.observableArray([]);
        crossService: CrossService;
        crossId: KnockoutObservable<string>;
        warriorCompleted: KnockoutComputed<boolean>;
        name: KnockoutComputed<string>;
        imgSrcAttr: KnockoutComputed<string>;

        constructor(params) {
            // 'params' is an object whose key/value pairs are the parameters
            // passed from the component binding or custom element.
            this.crossId = params.crossId;
            this.crossService = new CrossService();
            this.getCrossDetail(this.crossId())
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.userIsGuardian = params.userIsGuardian;
        
            this.crossId.subscribe(newValue => {
                this.getCrossDetail(newValue)
            });

            this.warriorCompleted = ko.pureComputed<boolean>(() => {
                return this.cross().warriorCompleted();
            });

            this.name = ko.pureComputed<string>(() => {
                return this.cross().name();
            });

            this.imgSrcAttr = ko.pureComputed<string>(() => {
                return this.cross().imgSrcAttr();
            });
        }

        daysCompleteMessage: KnockoutComputed<string> = ko.pureComputed<string>(() => {
            return `${this.days().filter(d => d.warriorCompleted()).length} of ${this.days().length} days completed`
        });

        percentWeightPerCheckpoint: KnockoutComputed<number> = ko.pureComputed<number>(() => {
            let result = 0;
            if (this.days() !== null) {
                result = this.days().filter(d => d.isCheckpoint()).length;
            }
            return result;
        })

        allDaysComplete = () => {
            return !this.days().some(d => d.completedAt() === null)
        }

        getCrossDetail = (id: string) => {
            let self = this;
            this.crossService.getCrossDetail(id,
                function (data: Cross) {
                    ko.mapping.fromJS(data, WarriorsGuild.KnockoutMapperConfigurations.koCrossMapperConfiguration, <KnockoutObservableType<Cross>><any>self.cross());
                    self.getCrossDays();
                });
        }

        getCrossDays = (): void => {
            var self = this;
            self.days.removeAll();
            this.crossService.getCrossDays(this.cross().id(),
                (data: CrossDay[]) => {
                    $.each(data, (i, day) => {
                        var oDay = new ObservableCrossDay();
                        oDay.approvedAt(WarriorsGuild.DateConversion.convertStringToNullableDate(day.approvedAt));
                        oDay.completedAt(WarriorsGuild.DateConversion.convertStringToNullableDate(day.completedAt));
                        self.days.push(oDay);
                    });
                });
        }
    }
}

ko.components.register('cross-summary', {
    viewModel: WarriorsGuild.CrossSummaryViewModel,
    template: `
                <h3><img style="height:40px" data-bind="attr: { src: imgSrcAttr, alt: name, title: name }" /><span data-bind="text: name"></span></h3>
                <h3 data-bind="if: allDaysComplete() && warriorCompleted()">Complete</h3>
                <div data-bind="if: !warriorCompleted()">
                    <h3 data-bind="text: daysCompleteMessage()"></h3>
                </div>
                <h3 data-bind="if: allDaysComplete() && !warriorCompleted()">Summary Pending</h3>
            `
});
