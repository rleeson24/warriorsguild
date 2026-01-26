namespace WarriorsGuild {
    export class ReadOnlyRankRequirementViewModel {
        rank: ObservableRank;
        requirement: ObservableRankRequirement;
        crossDay: CrossDay;
        question: ObservableCrossQuestion;
        hasActiveSubscription: boolean;
        userIsWarrior: boolean;
        showEntryField: boolean;
        constructor(params) {
            this.rank = params.rank;
            this.requirement = params.requirement;
            this.crossDay = params.crossDay;
            this.question = params.question;
            this.hasActiveSubscription = params.hasActiveSubscription;
            this.userIsWarrior = params.userIsWarrior;
            this.showEntryField = true;
        }
        // 'params' is an object whose key/value pairs are the parameters
        // passed from the component binding or custom element.

        actionToCompleteHtml = ko.computed(() => {
            return `${this.requirement.actionToCompleteLinked()}${this.requirement.optional() ? ' (Optional)' : ''}`
        })

        isOutOfReach = ko.computed(() => {
            return userIsWarrior && !this.requirement.warriorCompleted() && (this.rank.requestPromotionEnabled() || this.rank.pendingApproval());
        })

        guardianReviewed = ko.computed(() => this.requirement.guardianReviewed());
        warriorCompleted = ko.computed(() => this.requirement.warriorCompleted());
        completionSummaryText = ko.computed(() => this.requirement.completionSummaryText());
        requireRing = ko.computed(() => this.requirement.requireRing());
        savedRings = ko.computed(() => this.requirement.savedRings());
        requireCross = ko.computed(() => this.requirement.requireCross());
        requireAttachment = ko.computed(() => this.requirement.requireAttachment());

        downloadProofOfCompletionFile = (data: any): void => {
            WarriorsGuild.serviceBase.get({
                url: `/api/rankstatus/ProofOfCompletion/OneUseFileKey/${data.id}`,
                contentType: "application/json; charset=utf-8",
                success: function (oneTimeAccessToken: string) {
                    window.open(`/api/rankstatus/ProofOfCompletion}/${oneTimeAccessToken}`);
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Action Failed!",
                        message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            });
        }
    }
}

ko.components.register('read-only-rank-req', {
    viewModel: WarriorsGuild.ReadOnlyRankRequirementViewModel,
    template: `<div class="col-xs-12 text-left col-requirement">
                    <div class="check">
                        <div class="squaredThree" data-bind="css: { completed: guardianReviewed() }">
                            <div data-bind="attr: { id: 'rq' + $index(), checked: warriorCompleted() }"></div>
                            <label data-bind="attr: { for: 'rq' + $index(), title: completionSummaryText() }"></label>
                        </div>
                    </div>
                    <span class="actionToComplete" data-bind="html: actionToCompleteHtml(), css: { outOfReach: isOutOfReach() }"></span>
                    <div style="width:100%; height: 100%; opacity:.5"></div>
                </div>
                <div class="col-xs-12 col-sm-12 col-md-offset-2 col-md-8" data-bind="visible: requireRing() && warriorCompleted()">
                    <div class="rankAdditionalRingRequirements">
                        <label>Rings:</label>
                        <!-- ko foreach: savedRings() -->
                            <img style="height:40px" data-bind="attr: { src: imgSrcAttr, alt: name, title: name }" />
                        <!-- /ko -->
                    </div>
                </div>
                <div class="col-xs-12 col-sm-12 col-md-offset-2 col-md-8" data-bind="visible: requireCross() && warriorCompleted()">
                    <div class="row text-center" data-bind="foreach: crossesToComplete">
                        <img style="height:40px" data-bind="attr: { src: imgSrcAttr, alt: name, title: name }" />
                        <div data-bind="with: $parents[2].dataModel.DaysToComplete()[$index()]">
                            <cross-day params="{cross: $parentContext.$data,crossDay: $data,hasActiveSubscription: @HasActiveSubscription,userIsWarrior: @UserIsWarriorString }" />
                        </div>
                    </div>
                </div>
                <div class="col-xs-12 col-sm-12 col-md-offset-2 col-md-8" data-bind="visible: requireAttachment() && warriorCompleted()">
                    <div class="rankAdditionalRingRequirements">
                        <label>Attachments:</label>
                        <!-- ko foreach: attachments -->
                            <a data-bind="click: downloadProofOfCompletionFile" style="cursor: pointer" class="btn btn-sm btn-info">
                                <span data-bind="text:$index() + 1"></span>
                            </a>
                        <!-- /ko -->
                    </div>
                </div>`
});