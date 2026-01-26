namespace WarriorsGuild {
    export class Rank {
        id: string;
        name: string;
        description: string;
        imageUploaded: Date | null;
        requirements: [{
            id: string;
            actionToComplete: string;
        }];
        statuses: RankStatus[];
        percentComplete: number;
        guideUploaded: Date | null;
    }

    export class RankStatus {
        rankId: string;
        rankRequirementId: string;
        warriorCompleted: string | null;
        guardianCompleted: string | null;
    }

    export class MyRankViewModel {
        completedRank: Rank;
        workingRank: Rank;
        workingCompletionPercentage: number;
        completedCompletionPercentage: number;
    }

    export class ObservableRank {
        constructor() {
            var self = this;
            this.requestPromotionEnabled.subscribe(newValue => {
                if (newValue && !self.pendingApproval() && !this.warriorCompleted() && self.returnReason() == null) {
                    $('#requestPromotionPopup').modal('show');
                }
            });
            this.pendingApproval.subscribe(newValue => {
                if (newValue) {
                    $('#requestPromotionPopup').modal('hide');
                }
            });
        }

        id: KnockoutObservable<string> = ko.observable(null);
        name: KnockoutObservable<string> = ko.observable(null);
        description: KnockoutObservable<string> = ko.observable(null);
        imageUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
        imageExtension: KnockoutObservable<string> = ko.observable('');
        approvalRecordId: KnockoutObservable<string> = ko.observable(null);
        guardianApprovedTs: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
        completedTs: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
        highestPercentSubmittedForApproval: KnockoutObservable<number> = ko.observable(0);
        subRank: KnockoutObservable<string> = ko.observable(null);
        guideUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
        percentComplete: KnockoutObservable<number> = ko.observable<number>(0);
        returnReason: KnockoutObservable<string> = ko.observable<string>(null);

        requirements: KnockoutObservableArray<ObservableRankRequirement> = ko.observableArray<ObservableRankRequirement>([]);
        addRequirement(data: ObservableRank): void {
            data.requirements.push(new ObservableRankRequirement());
        };
        warriorCompleted = ko.pureComputed((): boolean => {
            return this.highestPercentSubmittedForApproval() === this.completedPercent();
        }, this);
        guardianApproved = ko.pureComputed((): boolean => {
            return this.guardianApprovedTs() !== null;
        }, this);
        pendingApproval = ko.pureComputed((): boolean => {
            return this.approvalRecordId() !== null && !this.guardianApproved();
        }, this);
        requestPromotionEnabled = ko.pureComputed((): boolean => {
            return (this.highestPercentSubmittedForApproval() < 100 && this.completedPercent() === 100)
                || this.completedPercent() - 33 >= (this.highestPercentSubmittedForApproval() >= 66 ? 66 : this.highestPercentSubmittedForApproval() >= 33 ? 33 : 0);
        }, this);
        hasImage: KnockoutComputed<boolean> = ko.pureComputed(() => {
            return this.imageUploaded() != null;
        }, this);
        hasGuide: KnockoutComputed<boolean> = ko.pureComputed(() => {
            return this.guideUploaded() !== null;
        }, this);
        completedPercent = ko.pureComputed(() => {
            var total = 0
            for (let req of this.requirements()) {
                if (req.warriorCompleted()) {
                    total += req.weight();
                }
                else if (req.requireCross) {
                    //read the requirement's cross percentCompleted
                    total += req.totalPercentComplete();
                    //if (req.totalPercentComplete() > 0) {
                    //    alert(req.totalPercentComplete())
                    //}
                }
            }
            return total;
        }, this);
        imgSrcAttr = ko.pureComputed<string>(() => { return this.hasImage() ? '/images/ranks/' + this.id() + this.imageExtension() + '?' + this.imageUploaded().getTime() : '/images/logo/Warriors-Guild-icon-sm-wide.png' }, this);
        getDetailLink = ko.pureComputed<string>(() => { return `/ranks#detail?id=${this.id()}`; });
    }

    export class RankRequirement extends BaseRequirement {
        rankId: string;
        requireRing: boolean;
        requiredRingType: string;
        requiredRingCount: number | null;
        requireCross: boolean;
        requiredCrossCount: number | null;
        savedRings: MinimumRingDetail[];
        savedCrosses: MinimumCrossDetail[];
        crossesToComplete: MinimumCrossDetail[];
        actionToCompleteLinked: string;
        optional: boolean;
        initiatedByGuardian: boolean;
        showAtPercent: number;
        visible: boolean = true;
    }

    export class ObservableRankRequirement {
        id: string = '';
        rankId: string = '';
        weight: KnockoutObservable<number> = ko.observable(0);
        actionToComplete: KnockoutObservable<string> = ko.observable('');
        markAsComplete: Function;
        revertCompletion: Function;
        warriorCompletedTs: KnockoutObservable<Date | null> = ko.observable(null);
        guardianReviewedTs: KnockoutObservable<Date | null> = ko.observable(null);
        requireAttachment: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        requireRing: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        requireCross: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        requiredCrossCount: KnockoutObservable<number | null> = ko.observable<number | null>(null);
        savedCrosses: KnockoutObservableArray<MinimumCrossDetail> = ko.observableArray<MinimumCrossDetail>([]);
        requiredRingType: KnockoutObservable<string> = ko.observable<string>(null);
        requiredRingCount: KnockoutObservable<number | null> = ko.observable<number | null>(null);
        savedRings: KnockoutObservableArray<MinimumRingDetail> = ko.observableArray<MinimumRingDetail>([]);
        optional: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        initiatedByGuardian: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        showAtPercent: KnockoutObservable<number> = ko.observable<number>(0);
        visible: KnockoutObservable<boolean> = ko.observable<boolean>(true);

        crossesToComplete: KnockoutObservableArray<MinimumCrossDetail> = ko.observableArray<MinimumCrossDetail>([]);
        totalPercentComplete: KnockoutObservable<number> = ko.observable<number>(0);

        selectedCrosses: KnockoutObservableArray<MinimumCrossDetail> = ko.observableArray<MinimumCrossDetail>([]);
        selectedRings: KnockoutObservableArray<MinimumRingDetail> = ko.observableArray<MinimumRingDetail>([]);
        attachments: KnockoutObservableArray<{ id: KnockoutObservable<string> }> = ko.observableArray<{ id: KnockoutObservable<string> }>([]);
        seeHowLink: KnockoutObservable<string> = ko.observable<string>(null);

        seeHowLinkInvalid: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => { return this.seeHowLink() !== null && this.seeHowLink().length !== 0 && !WarriorsGuild.isValidUrl(this.seeHowLink()) }, this);
        actionToCompleteLinked: KnockoutComputed<string> = ko.pureComputed<string>(() => { return this.actionToComplete()?.replace(/\n/g, '<br />').replace(/\[link ([a-zA-Z ]+)\]/g, `<a href='${this.seeHowLink()}' target='_blank'>$1</a>`); });

        constructor() {
            const self = this;
            if (this.requireAttachment()) {
                this.warriorCompletedTs.subscribe(d => {
                    if (d !== null) {
                        return false;
                    }
                });
            }
            this.requireCross.subscribe(d => {
                if (d) {
                    this.requiredCrossCount(1);
                }
                else {
                    this.requiredCrossCount(0);
                }
            });
            this.requiredCrossCount.subscribe(d => {
                var existing = self.crossesToComplete().slice(0, d);
                self.crossesToComplete(existing)
                var itemsToConcat = d - existing.length;
                for (let i = 0; i < itemsToConcat; i++) {
                    self.crossesToComplete.push(new MinimumCrossDetail());
                }
            });
        }
        formatDateTime = (date) => {
            var ts = date;
            var result = '';
            if (ts != null) {
                if (ts() !== null) {
                    var hours = ("0" + ts().getHours());
                    hours = hours.substr(hours.length - 2, 2);
                    var minutes = ("0" + ts().getMinutes());
                    minutes = minutes.substr(minutes.length - 2, 2);
                    result = ts().toLocaleDateString() + " " + hours + ":" + minutes;
                }
            }
            return result;
        };
        formattedWarriorCompletedTs = ko.computed(() => {
            return this.formatDateTime(this.warriorCompletedTs);
        }, this);
        formattedGuardianReviewedTs = ko.computed(() => {
            return this.formatDateTime(this.guardianReviewedTs);
        }, this);
        warriorCompleted = ko.computed(() => {
            return this.warriorCompletedTs ? this.warriorCompletedTs() !== null : false;
        }, this);
        guardianReviewed = ko.computed(() => {
            return this.guardianReviewedTs ? this.guardianReviewedTs() !== null : false;
        }, this);
        completionSummaryText = ko.computed(() => {
            let result = '';
            if (this.warriorCompleted()) {
                result += 'Completed at ' + this.formattedWarriorCompletedTs();
                if (this.guardianReviewed())
                    result += ';  Confirmed at ' + this.formattedGuardianReviewedTs();
            }
            return result;
        }, this);
        shouldBeDisabled = ko.computed(() => {
            return this.guardianReviewed();// || ( /*confirmationPending*/ false && !this.warriorCompleted() );
        }, this);
        addAttachment = (attIds: string[]) => {
            $.each(attIds, (index: number, att: string) => {
                this.attachments.push({ id: ko.observable<string>(att) });
            });
        };
    }

    export class PendingRankApproval {
        approvalRecordId: string = '';
        rankId: string = '';
        rankName: string = '';
        percentComplete: number = 0;
        rankImageUploaded: Date | null = null;
        warriorCompletedTs: string;
        guardianApprovedTs: string;
        unconfirmedRequirements: RankRequirement[] = new RankRequirement[0];
        hasImage: boolean;
        pendingSubRank: string;
        guideUploaded: string;
        imageExtension: string;
        imgSrcAttr: string;
        returnedTs: Date | null = null;
        returnReason: string;
    }

    //export class UnassignedCrossModel {
    //	name: string = '';
    //	id: string = '';
    //	imageExtension: string = '';
    //	imageExtension: string = '';
    //}

    //export class UnassignedRingModel {
    //	name: KnockoutObservable<string> = ko.observable('');
    //	id: KnockoutObservable<string> = ko.observable('');
    //	type: KnockoutObservable<string> = ko.observable('');
    //}

    export class MinimumGoalDetail {
        name: string = '';
        imageExtension: string = '';
        hasImage: boolean = false;
        imgSrcAttr: string = '';
        percentCompleted: number = 0;
    }

    export class MinimumRingDetail extends MinimumGoalDetail {
        ringId: string = '';
        type: string = '';
    }

    export class MinimumCrossDetail extends MinimumGoalDetail {
        crossId: KnockoutObservable<string>;
        constructor() {
            super();
            this.crossId = ko.observable<string>('')
        }
    }
}