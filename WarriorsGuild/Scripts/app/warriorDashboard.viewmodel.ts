declare var warriorDashboardViewModel: {
    readOnly: boolean;
    profileId: string;
    getCurrentAndWorkingRank: string;
    getPendingApproval: string;
    markAsComplete: string;
    rankImageBase: string;
    fullName: string;
    avatarSrc: string;
    favoriteVerse: string;
    hobbies: string;
    interestingFact: string;
    favoriteMovie: string;
    imageUploaded: Date;
    currentAndWorkingRank: WarriorsGuild.MyRankViewModel;
    ranksUrl: string;
    rankStatusUrl: string;
    ringStatusUrl: string;
    crossStatusUrl: string;
    crossUrl: string;
    proofOfCompletionUrl: string;
};

namespace WarriorsGuild {
    export class WarriorDashboardViewModel {
        app: AppViewModel;
        dataModel: {
            BlogEntries: KnockoutObservableArray<object>;
            CompletedRank: KnockoutObservable<ObservableRank>;
            WorkingRank: KnockoutObservable<ObservableRank>;
            CompletedCompletionPercentage: KnockoutObservable<number>;
            WorkingCompletionPercentage: KnockoutObservable<number>;
            PinnedRings: KnockoutObservableArray<MinimumRingDetail>;
            CompletedSilverRings: KnockoutObservableArray<ObservableRing>;
            CompletedGoldRings: KnockoutObservableArray<ObservableRing>;
            CompletedPlatinumRings: KnockoutObservableArray<ObservableRing>;
            PinnedCrosses: KnockoutObservableArray<MinimumCrossDetail>;
            CompletedCrosses: KnockoutObservableArray<ObservableCross>;
            PendingRankApproval: KnockoutObservable<PendingRankApproval>;
            OnLockDown: KnockoutComputed<Boolean>;
            silverRingOptions: KnockoutObservableArray<MinimumRingDetail>;
            goldRingOptions: KnockoutObservableArray<MinimumRingDetail>;
            platinumRingOptions: KnockoutObservableArray<MinimumRingDetail>;
            crossOptions: KnockoutObservableArray<MinimumCrossDetail>;
            crossesToComplete: KnockoutObservableArray<MinimumCrossDetail>;
            PendingCrossApprovals: KnockoutObservableArray<PendingCrossApproval>;
        };
        RetrievingRank: KnockoutObservable<boolean>;
        RetrievingPinnedRings: KnockoutObservable<boolean>;
        RetrievingPinnedCrosses: KnockoutObservable<boolean>;
        RetrievingPinnedRingsError: KnockoutObservable<boolean>;
        profileId: KnockoutObservable<string> = ko.observable<string>(null);
        rankService: RankService;
        constructor(app: WarriorsGuild.AppViewModel, dataModel) {
            var self = this;
            self.app = app;
            this.rankService = new WarriorsGuild.RankService();
            this.dataModel = {
                BlogEntries: ko.observableArray([]),
                CompletedRank: ko.observable<ObservableRank>(new ObservableRank()),
                WorkingRank: ko.observable<ObservableRank>(new ObservableRank()),
                CompletedCompletionPercentage: ko.observable<number>(0),
                WorkingCompletionPercentage: ko.observable<number>(0),
                PinnedRings: ko.observableArray<MinimumRingDetail>([]),
                CompletedSilverRings: ko.observableArray<ObservableRing>([]),
                CompletedGoldRings: ko.observableArray<ObservableRing>([]),
                CompletedPlatinumRings: ko.observableArray<ObservableRing>([]),
                PinnedCrosses: ko.observableArray<MinimumCrossDetail>([]),
                CompletedCrosses: ko.observableArray<ObservableCross>([]),
                PendingRankApproval: ko.observable<PendingRankApproval>(),
                OnLockDown: ko.pureComputed<boolean>(function () { return true; }),
                silverRingOptions: ko.observableArray<MinimumRingDetail>(),
                goldRingOptions: ko.observableArray<MinimumRingDetail>(),
                platinumRingOptions: ko.observableArray<MinimumRingDetail>(),
                crossOptions: ko.observableArray<MinimumCrossDetail>(),
                crossesToComplete: ko.observableArray<MinimumCrossDetail>(),
                PendingCrossApprovals: ko.observableArray<PendingCrossApproval>([])
            };
            self.dataModel.OnLockDown = ko.pureComputed<boolean>(function () {
                return this.PendingRankApproval() != null;
            }, self.dataModel);
            self.RetrievingRank = ko.observable(false);
            self.RetrievingPinnedRings = ko.observable(false);
            self.RetrievingPinnedCrosses = ko.observable(false);
            self.RetrievingPinnedRingsError = ko.observable(false)
            self.dataModel.BlogEntries = ko.observableArray([]);
            app.prepareAjax();
            Sammy(function () {
                this.get('#home', function () {
                    // Make a call to get blog entries
                    //$.ajax( {
                    //	method: 'get',
                    //	url: 'https://public-api.wordpress.com/rest/v1.1/sites/warriorsguild.blog/posts/?number=2',
                    //	contentType: "application/json; charset=utf-8",
                    //	success: function ( data ) {
                    //		$.each( data.posts, function ( index, data ) {
                    //			self.dataModel.BlogEntries.push( data );
                    //		} );
                    //	}
                    //} );
                    self.GetProfileData();
                    //self.setCurrentAndWorkingRanks( warriorDashboardViewModel.currentAndWorkingRank );
                });
                this.get('/profile/:id', function () {
                    var currContext = this as Sammy.EventContext;
                    self.profileId(currContext.params.id);
                    this.app.runRoute('get', '#home');
                });
                this.get('/Profile/:id', function () {
                    var currContext = this as Sammy.EventContext;
                    self.profileId(currContext.params.id);
                    this.app.runRoute('get', '#home');
                });
                this.get('/Dashboard/Warrior', function () { this.app.runRoute('get', '#home'); });
                this.get('/dashboard/warrior', function () { this.app.runRoute('get', '#home'); });
                this.get('/Dashboard', function () { this.app.runRoute('get', '#home'); });
            });
        }

        GetProfileData = (): void => {
            var self = this;
            self.RetrievingRank(true);
            self.RetrievingPinnedRings(true);
            self.RetrievingPinnedCrosses(true);
            //Make a call to the protected Web API by passing in a Bearer Authorization Header
            $.ajax({
                method: 'get',
                url: warriorDashboardViewModel.getPendingApproval,
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: PendingRankApproval) {
                    if (data != null) {
                        data.hasImage = data.rankImageUploaded != null;
                    }
                    self.dataModel.PendingRankApproval(data);
                },
                error: function (err: JQueryXHR) {
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Pending Approvals Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred attempting to retrieve rank approval"
                        });
                    }
                }
            });
            this.getWorkingAndCompletedRank();
            $.ajax({
                method: 'get',
                url: '/api/rings/pinned/active',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: PinnedRing[]) {
                    $.each(data, function (i, r) {
                        let m = new MinimumRingDetail();
                        m.hasImage = r.imageUploaded !== null;
                        m.ringId = r.ring.id;
                        m.imageExtension = r.ring.imageExtension;
                        m.name = r.ring.name;
                        m.percentCompleted = r.percentComplete;
                        m.imgSrcAttr = m.hasImage ? "/images/rings/" + r.ring.id + r.ring.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png"
                        self.dataModel.PinnedRings.push(m);
                    });
                    self.RetrievingPinnedRings(false);
                },
                error: function (err: JQueryXHR) {
                    self.RetrievingPinnedRings(false);
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving Pinned Rings"
                        });
                    }
                }
            });
            $.ajax({
                method: 'get',
                url: '/api/rings/completed',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: Ring[]) {
                    var silver = 'Silver', gold = 'Gold', platinum = 'Platinum';
                    $.each(data.sort((a, b) => a.name.localeCompare(b.name)), function (i, v) {
                        var oRing = <ObservableRing><any>ko.mapping.fromJS(v, self.koMapperConfiguration, <KnockoutObservableType<Ring>><any>new ObservableRing());
                        switch (v.type) {
                            case silver:
                                self.dataModel.CompletedSilverRings.push(oRing);
                                break;
                            case gold:
                                self.dataModel.CompletedGoldRings.push(oRing);
                                break;
                            case platinum:
                                self.dataModel.CompletedPlatinumRings.push(oRing);
                                break;
                        }
                    });
                },
                error: function (err: JQueryXHR) {
                    self.RetrievingPinnedRings(false);
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving completed rings"
                        });
                    }
                }
            });
            $.ajax({
                method: 'get',
                url: '/api/crosses/pinned/active',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: PinnedCross[]) {
                    $.each(data, function (i, c) {
                        let m = new MinimumCrossDetail();
                        m.hasImage = c.cross.imageUploaded !== null;
                        m.crossId(c.cross.id);
                        m.imageExtension = c.cross.imageExtension;
                        m.name = c.cross.name;
                        m.percentCompleted = c.percentComplete;
                        m.imgSrcAttr = m.hasImage ? "/images/crosses/" + c.cross.id + c.cross.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png"
                        self.dataModel.PinnedCrosses.push(m);
                    });
                    self.RetrievingPinnedCrosses(false);
                },
                error: function (err: JQueryXHR) {
                    self.RetrievingPinnedCrosses(false);
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving Pinned Crosses"
                        });
                    }
                }
            });
            $.ajax({
                method: 'get',
                url: '/api/crosses/completed',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: Cross[]) {
                    $.each(data.sort((a, b) => a.name.localeCompare(b.name)), function (i, v) {
                        var oCross = <ObservableCross><any>ko.mapping.fromJS(v, self.koMapperConfiguration, <KnockoutObservableType<Cross>><any>new ObservableCross());
                        self.dataModel.CompletedCrosses.push(oCross);
                    });
                },
                error: function (err: JQueryXHR) {
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving completed crosses"
                        });
                    }
                }
            });
        };

        getWorkingAndCompletedRank = (): void => {
            var self = this;
            $.ajax({
                method: 'get',
                url: warriorDashboardViewModel.getCurrentAndWorkingRank,
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: MyRankViewModel) {
                    self.RetrievingRank(false);
                    self.setCurrentAndWorkingRanks(data);
                },
                error: function (err: JQueryXHR) {
                    self.RetrievingRank(false);
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred attempting to retrieve your rank"
                        });
                    }
                }
            });
        }

        setCurrentAndWorkingRanks = (data: MyRankViewModel): void => {
            var self = this;
            if (data.completedRank !== null) {
                ko.mapping.fromJS(data.completedRank, self.koMapperConfiguration, <KnockoutObservableType<Rank>><any>self.dataModel.CompletedRank);
                self.dataModel.CompletedRank().subRank(self.calculateSubRank(data.completedRank.percentComplete));
            }
            self.dataModel.WorkingRank().requirements.removeAll();
            ko.mapping.fromJS(data.workingRank, self.koMapperConfiguration, <KnockoutObservableType<Rank>><any>self.dataModel.WorkingRank);
            self.retrieveRequirements(self.dataModel.WorkingRank());
            self.retrieveApprovalStatus(self.dataModel.WorkingRank());
            self.dataModel.WorkingCompletionPercentage(data.workingCompletionPercentage);
            self.dataModel.CompletedCompletionPercentage(data.completedCompletionPercentage);
        };

        completionPercentUpdatedHandler = (data: ObservableRankRequirement, percentComplete: number): void => {
            var requirementCompletion = data.weight() * percentComplete / 100;
            data.totalPercentComplete(requirementCompletion);
            if (percentComplete === 100 && data.warriorCompletedTs() === null) {
                this.markAsComplete(data);
            }
            else if (percentComplete < 100 && data.warriorCompletedTs() !== null) {
                this.revertCompletion(data);
            }
        }

        private koMapperConfiguration = (() => {
            var self = this;
            return {
                'completed': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'confirmed': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'imageUploaded': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'guideUploaded': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'warriorCompletedTs': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'guardianReviewedTs': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'guardianApprovedTs': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'requirements': {
                    create: function (options) {
                        return ko.mapping.fromJS(options.data, {}, new ObservableRankRequirement());
                    }
                }
            };
        })();

        retrieveRequirements = (rank: ObservableRank): void => {
            const cmpBoolean = (x, y): number => {
                return (x === y) ? 0 : x ? -1 : 1;
            }
            var self = this;
            this.rankService.retrieveRankRequirements(rank.id(),
                (requirements: RankRequirement[]) => {
                    rank.requirements.removeAll();
                    var reqs = requirements.filter(r => !r.requireCross && !r.warriorCompleted);
                    reqs.push(...requirements.filter(r => r.requireCross).sort((x,y) => cmpBoolean(x.warriorCompleted, y.warriorCompleted)));
                    reqs.push(...requirements.filter(r => !r.requireCross && r.warriorCompleted));
                    let uncompletedCrossReq = null;
                    reqs.forEach(rr => {
                        if (rr.requireCross && !rr.warriorCompleted) {
                            if (uncompletedCrossReq === null)
                                uncompletedCrossReq = rr;
                            else
                                rr.visible = false;
                        }
                        else if (rr.initiatedByGuardian && !rr.warriorCompleted) {
                            rr.visible = false;
                        }
                        else {
                            rr.visible = true;
                        }
                    });
                    ko.mapping.fromJS(reqs, <KnockoutMappingOptions<RankRequirement[]>>{
                        create: function (options: KnockoutMappingCreateOptions) {
                            return self.CreateObservableRankRequirement(rank.id(), options.data);
                        }
                    }, <KnockoutObservableArray<KnockoutObservableType<RankRequirement>>><any>rank.requirements);
                    reqs.forEach((el): boolean => {
                        if (el.requireRing) {
                            self.retrieveUnassignedRings();
                            return false;
                        }
                    });
                    reqs.forEach(el => {
                        if (el.requireCross) {
                            self.retrieveUnassignedCrosses();
                            return false;
                        }
                    });
                }, (err: JQueryXHR) => {
                    BootstrapAlert.alert({
                        title: "Requirement Retrieval Failed!",
                        message: "Could not retrieve requirements" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            );
        }

        retrieveUnassignedRings = (): void => {
            var self = this;
            self.dataModel.silverRingOptions.removeAll();
            self.dataModel.goldRingOptions.removeAll();
            self.dataModel.platinumRingOptions.removeAll();
            $.ajax({
                method: 'get',
                url: warriorDashboardViewModel.ringStatusUrl + '/unassigned',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: MinimumRingDetail[]) {
                    $.each(data, (i, val) => {
                        switch (val.type) {
                            case "Silver":
                                self.dataModel.silverRingOptions.push(val);
                                break;
                            case "Gold":
                                self.dataModel.goldRingOptions.push(val);
                                break;
                            case "Platinum":
                                self.dataModel.platinumRingOptions.push(val);
                                break;
                        }
                    });
                },
                error: function (err) {
                    BootstrapAlert.alert({
                        title: "Unassigned Rings Retrieval Failed!",
                        message: "Could not retrieve unassigned rings"
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                }
            });
        }

        retrieveUnassignedCrosses = (): void => {
            var self = this;
            self.dataModel.crossOptions.removeAll();
            $.ajax({
                method: 'get',
                url: warriorDashboardViewModel.crossUrl + '/unassigned',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: MinimumCrossDetail[]) {
                    $.each(data, (i, val) => {
                        self.dataModel.crossOptions.push(val);
                    });
                },
                error: function (err) {
                    BootstrapAlert.alert({
                        title: "Unassigned Crosses Retrieval Failed!",
                        message: "Could not retrieve unassigned crosses"
                    });
                }
            });
        }

        nullableDate(dateString: string): Date {
            return dateString === null ? null : new Date(dateString);
        }

        retrieveApprovalStatus = (rank: ObservableRank): void => {
            var self = this;
            $.ajax({
                method: 'get',
                url: warriorDashboardViewModel.rankStatusUrl + '/approvalsForRank/' + rank.id(),
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (data: PendingRankApproval[]) {
                    data = data.sort((a, b) => b.warriorCompletedTs.localeCompare(a.warriorCompletedTs));
                    let pendingApproval = data.filter(a => a.returnedTs == null && a.guardianApprovedTs == null);
                    if (pendingApproval.length) {
                        rank.highestPercentSubmittedForApproval(pendingApproval[0].percentComplete);
                        rank.approvalRecordId(pendingApproval[0].approvalRecordId);
                        rank.completedTs(self.nullableDate(pendingApproval[0].warriorCompletedTs));
                        rank.guardianApprovedTs(self.nullableDate(pendingApproval[0].guardianApprovedTs));
                        rank.guideUploaded(self.nullableDate(pendingApproval[0].guideUploaded));
                    }
                    else {
                        rank.highestPercentSubmittedForApproval(0);
                        rank.approvalRecordId(null);
                        rank.completedTs(null);
                        rank.guardianApprovedTs(null);
                        rank.guideUploaded(null);
                    }
                    let rejectedApproval = data[0];
                    if (rejectedApproval?.returnedTs) {
                        rank.returnReason(rejectedApproval.returnReason);
                    }
                },
                error: function (err) {
                    BootstrapAlert.alert({
                        title: "Requirement Retrieval Failed!",
                        message: "Could not retrieve requirements"
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                }
            });
        }

        markAsComplete = (data: ObservableRankRequirement): void => {
            if (!warriorDashboardViewModel.readOnly) {
                if (!data.requireAttachment()) {
                    var self = this;
                    var validationMessages = self.validateMarkAsCompleteRequest(data);
                    if (validationMessages.length === 0) {
                        //var selectedRings = data.selectedRings().map(ring => ring.ringId);
                        //var selectedCrosses = data.selectedCrosses().map(cross => cross.crossId);
                        this.rankService.markRequirementComplete(data.rankId, data.id,
                            (rank: Rank) => {
                                BootstrapAlert.success({
                                    title: "Action Success!",
                                    message: "The requirement has been marked as complete"
                                });
                                if (!data.warriorCompleted()) {
                                    data.warriorCompletedTs(new Date());
                                }
                                else {
                                    data.guardianReviewedTs(new Date());
                                }
                            },
                            (err: JQueryXHR) => {
                                var errFromServer = WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err);
                                BootstrapAlert.alert({
                                    title: "Action Failed!",
                                    message: "Could not mark the given requirement as complete" + errFromServer
                                });
                            }
                        );
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Request is not valid. " + validationMessages.join('. ') + '.'
                        });
                    }
                }
            }
        }

        validateMarkAsCompleteRequest = (data: ObservableRankRequirement): string[] => {
            return [];
            var messages = new Array<string>(0);
            if (data.requireRing()) {
                if (data.selectedRings().length === data.requiredRingCount()) {
                    var selectedRings = new Array(0);
                    $.each(data.selectedRings(), (i, val) => {
                        if (val.ringId === null || val.ringId === undefined || val.ringId.length !== 36) {
                            messages.push('One or more ring(s) does not have a valid selection');
                            return false;
                        }
                        else {
                            if ($.grep(selectedRings, (el, i): boolean => {
                                return el === val.ringId;
                            }).length === 0) {
                                selectedRings.push(val.ringId);
                            }
                            else {
                                messages.push('All selected rings must be unique');
                            }
                        }
                    });
                }
                else {
                    messages.push('The ring count does not meet the requirements');
                }
            }
            if (data.requireCross()) {
                if (data.selectedCrosses().length === data.requiredCrossCount()) {
                    var selectedCrosses = new Array(0);
                    $.each(data.selectedCrosses(), (i, val) => {
                        if (val.crossId === null || val.crossId === undefined || val.crossId.length !== 36) {
                            messages.push('One or more cross(es) does not have a valid selection');
                            return false;
                        }
                        else {
                            if ($.grep(selectedCrosses, (el, i): boolean => {
                                return el === val.crossId;
                            }).length === 0) {
                                selectedCrosses.push(val.crossId);
                            }
                            else {
                                messages.push('All selected crosses must be unique');
                            }
                        }
                    });
                }
                else {
                    messages.push('The cross count does not meet the requirements');
                }
            }
            return messages;
        }

        downloadProofOfCompletionFile = (data: any): void => {
            var self = this;
            $.ajax({
                method: 'get',
                url: warriorDashboardViewModel.proofOfCompletionUrl + '/OneUseFileKey/' + data.id(),
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (oneTimeAccessToken: string) {
                    window.open(`${warriorDashboardViewModel.proofOfCompletionUrl}/${oneTimeAccessToken}`);
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Action Failed!",
                        message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            });
        }


        revertCompletion = (data: ObservableRankRequirement): void => {
            var self = this;
            $.ajax({
                method: 'delete',
                url: warriorDashboardViewModel.rankStatusUrl,
                data: ko.toJSON({ RankRequirementId: data.id, RankId: data.rankId }),
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (rank: Rank) {
                    BootstrapAlert.success({
                        title: "Action Success!",
                        message: "The requirement has been marked as incomplete"
                    });
                    self.retrieveUnassignedCrosses();
                    self.retrieveUnassignedRings();
                    data.savedCrosses.removeAll();
                    data.savedRings.removeAll();
                    data.attachments.removeAll();
                    data.warriorCompletedTs(null);
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Action Failed!",
                        message: "Could not mark the given requirement as incomplete"
                    });
                }
            });
        }

        rankForPromotion: ObservableRank = null;
        beginRequestForPromotion = (data: ObservableRank): void => {
            this.rankForPromotion = data;
            $('#requestPromotionPopup').modal('show');
        }

        submitForPromotion = (): void => {
            var self = this;
            $('#requestPromotionPopup').modal('hide');
            $.ajax({
                method: 'post',
                url: warriorDashboardViewModel.rankStatusUrl + '/SubmitForApproval/' + self.rankForPromotion.id(),
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function (rank: Rank) {
                    BootstrapAlert.success({
                        title: "Action Success!",
                        message: "Rank submitted for promotion"
                    });
                    self.getWorkingAndCompletedRank();
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Action Failed!",
                        message: "Could not submit this Rank for promotion"
                    });
                }
            });
        }

        recallRequestForPromotion = (data: ObservableRank): void => {
            var self = this;
            $.ajax({
                method: 'post',
                url: warriorDashboardViewModel.rankStatusUrl + '/' + data.approvalRecordId() + '/return',
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function () {
                    self.getWorkingAndCompletedRank();
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This rank has been recalled"
                    });
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The rank could not be recalled" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            });
        }

        returnCross = (data: ObservableCross): void => {
            var self = this;
            if (!data.warriorCompleted())
                return;
            $.ajax({
                method: 'post',
                url: `${warriorDashboardViewModel.crossStatusUrl}${data.id()}/return?reason=test`,
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function () {
                    data.completedAt(null);
                    self.dataModel.PendingCrossApprovals.removeAll();
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This cross has been returned"
                    });
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The cross could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                }
            });
        }

        CreateObservableRankRequirement = (rankId: string, data: RankRequirement): ObservableRankRequirement => {
            var self = this;
            var result = new ObservableRankRequirement();
            result.id = data.id;
            result.rankId = rankId;
            result.weight(data.weight);
            result.actionToComplete(data.actionToComplete);
            result.markAsComplete = self.markAsComplete;
            result.revertCompletion = self.revertCompletion;
            result.warriorCompletedTs(data.warriorCompleted !== null ? new Date(data.warriorCompleted) : null);
            result.guardianReviewedTs(data.guardianCompleted !== null ? new Date(data.guardianCompleted) : null);
            result.requireAttachment(data.requireAttachment);
            if (data.requireCross) {
                var totalComplete = 0;
                data.crossesToComplete.forEach(c => {
                    totalComplete += c.percentCompleted;
                    result.crossesToComplete.push(c);
                });
                var requirementCompletion = data.weight * totalComplete / 100;
                result.totalPercentComplete(requirementCompletion);
            }
            result.requireCross(data.requireCross);

            result.requiredCrossCount(data.requiredCrossCount);
            result.requireRing(data.requireRing);
            result.requiredRingCount(data.requiredRingCount);
            result.requiredRingType(data.requiredRingType);
            result.seeHowLink(data.seeHowLink);
            result.optional(data.optional);
            result.visible(data.visible ?? true);
            result.initiatedByGuardian(data.initiatedByGuardian);
            result.showAtPercent(data.showAtPercent);

            if (data.requiredRingCount != null) {
                for (var i = 0; i < data.requiredRingCount; i++) {
                    result.selectedRings.push(new MinimumRingDetail());
                }
            }

            if (data.requiredCrossCount != null) {
                for (var i = 0; i < data.requiredCrossCount; i++) {
                    result.selectedCrosses.push(new MinimumCrossDetail());
                }
            }

            $.each(data.savedRings, (i, r) => {
                result.savedRings.push(r);
            });

            $.each(data.savedCrosses, (i, r) => {
                result.savedCrosses.push(r);
            });

            $.each(data.attachments, (i, r) => {
                result.attachments.push(ko.mapping.fromJS(r));
            });
            return result;
        }

        calculateSubRank = (percentComplete: number): string => {
            if (percentComplete === 100) {
                return 'Master';
            }
            else if (percentComplete >= 66) {
                return 'Journeyman';
            }
            else if (percentComplete >= 33) {
                return 'Apprentice';
            }
        }

        showPopup = (popupId: string, warriorId: string) => {
            $.each($(`.popuptext:not(#${popupId}${warriorId})`), (index, el) => {
                el.classList.add('hidden');
            });
            const popupElement = document.getElementById(popupId + warriorId);
            popupElement ? popupElement.classList.add("hidden") : null;
            document.getElementsByClassName("ui-widget-overlay")[0].classList.add("hidden");
        };

        closePopup = (popupId: string, warriorId: string) => {
            const popupElement = document.getElementById(popupId);
            popupElement.classList.add("hidden");
            document.getElementsByClassName("ui-widget-overlay")[0].classList.add("hidden");
        };
    }
}

WarriorsGuild.app.addViewModel({
    name: "Dashboard",
    bindingMemberName: "dashboard",
    factory: WarriorsGuild.WarriorDashboardViewModel,
    allowUnauthorized: true
});
