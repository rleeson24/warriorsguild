var WarriorsGuild;
(function (WarriorsGuild) {
    var WarriorDashboardViewModel = /** @class */ (function () {
        function WarriorDashboardViewModel(app, dataModel) {
            var _this = this;
            this.profileId = ko.observable(null);
            this.GetProfileData = function () {
                var self = _this;
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
                    success: function (data) {
                        if (data != null) {
                            data.hasImage = data.rankImageUploaded != null;
                        }
                        self.dataModel.PendingRankApproval(data);
                    },
                    error: function (err) {
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
                _this.getWorkingAndCompletedRank();
                $.ajax({
                    method: 'get',
                    url: '/api/rings/pinned/active',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data, function (i, r) {
                            var m = new WarriorsGuild.MinimumRingDetail();
                            m.hasImage = r.imageUploaded !== null;
                            m.ringId = r.ring.id;
                            m.imageExtension = r.ring.imageExtension;
                            m.name = r.ring.name;
                            m.percentCompleted = r.percentComplete;
                            m.imgSrcAttr = m.hasImage ? "/images/rings/" + r.ring.id + r.ring.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
                            self.dataModel.PinnedRings.push(m);
                        });
                        self.RetrievingPinnedRings(false);
                    },
                    error: function (err) {
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
                    success: function (data) {
                        var silver = 'Silver', gold = 'Gold', platinum = 'Platinum';
                        $.each(data.sort(function (a, b) { return a.name.localeCompare(b.name); }), function (i, v) {
                            var oRing = ko.mapping.fromJS(v, self.koMapperConfiguration, new WarriorsGuild.ObservableRing());
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
                    error: function (err) {
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
                    success: function (data) {
                        $.each(data, function (i, c) {
                            var m = new WarriorsGuild.MinimumCrossDetail();
                            m.hasImage = c.cross.imageUploaded !== null;
                            m.crossId(c.cross.id);
                            m.imageExtension = c.cross.imageExtension;
                            m.name = c.cross.name;
                            m.percentCompleted = c.percentComplete;
                            m.imgSrcAttr = m.hasImage ? "/images/crosses/" + c.cross.id + c.cross.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
                            self.dataModel.PinnedCrosses.push(m);
                        });
                        self.RetrievingPinnedCrosses(false);
                    },
                    error: function (err) {
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
                    success: function (data) {
                        $.each(data.sort(function (a, b) { return a.name.localeCompare(b.name); }), function (i, v) {
                            var oCross = ko.mapping.fromJS(v, self.koMapperConfiguration, new WarriorsGuild.ObservableCross());
                            self.dataModel.CompletedCrosses.push(oCross);
                        });
                    },
                    error: function (err) {
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
            this.getWorkingAndCompletedRank = function () {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: warriorDashboardViewModel.getCurrentAndWorkingRank,
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        self.RetrievingRank(false);
                        self.setCurrentAndWorkingRanks(data);
                    },
                    error: function (err) {
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
            };
            this.setCurrentAndWorkingRanks = function (data) {
                var self = _this;
                if (data.completedRank !== null) {
                    ko.mapping.fromJS(data.completedRank, self.koMapperConfiguration, self.dataModel.CompletedRank);
                    self.dataModel.CompletedRank().subRank(self.calculateSubRank(data.completedRank.percentComplete));
                }
                self.dataModel.WorkingRank().requirements.removeAll();
                ko.mapping.fromJS(data.workingRank, self.koMapperConfiguration, self.dataModel.WorkingRank);
                self.retrieveRequirements(self.dataModel.WorkingRank());
                self.retrieveApprovalStatus(self.dataModel.WorkingRank());
                self.dataModel.WorkingCompletionPercentage(data.workingCompletionPercentage);
                self.dataModel.CompletedCompletionPercentage(data.completedCompletionPercentage);
            };
            this.completionPercentUpdatedHandler = function (data, percentComplete) {
                var requirementCompletion = data.weight() * percentComplete / 100;
                data.totalPercentComplete(requirementCompletion);
                if (percentComplete === 100 && data.warriorCompletedTs() === null) {
                    _this.markAsComplete(data);
                }
                else if (percentComplete < 100 && data.warriorCompletedTs() !== null) {
                    _this.revertCompletion(data);
                }
            };
            this.koMapperConfiguration = (function () {
                var self = _this;
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
                            return ko.mapping.fromJS(options.data, {}, new WarriorsGuild.ObservableRankRequirement());
                        }
                    }
                };
            })();
            this.retrieveRequirements = function (rank) {
                var cmpBoolean = function (x, y) {
                    return (x === y) ? 0 : x ? -1 : 1;
                };
                var self = _this;
                _this.rankService.retrieveRankRequirements(rank.id(), function (requirements) {
                    rank.requirements.removeAll();
                    var reqs = requirements.filter(function (r) { return !r.requireCross && !r.warriorCompleted; });
                    reqs.push.apply(reqs, requirements.filter(function (r) { return r.requireCross; }).sort(function (x, y) { return cmpBoolean(x.warriorCompleted, y.warriorCompleted); }));
                    reqs.push.apply(reqs, requirements.filter(function (r) { return !r.requireCross && r.warriorCompleted; }));
                    var uncompletedCrossReq = null;
                    reqs.forEach(function (rr) {
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
                    ko.mapping.fromJS(reqs, {
                        create: function (options) {
                            return self.CreateObservableRankRequirement(rank.id(), options.data);
                        }
                    }, rank.requirements);
                    reqs.forEach(function (el) {
                        if (el.requireRing) {
                            self.retrieveUnassignedRings();
                            return false;
                        }
                    });
                    reqs.forEach(function (el) {
                        if (el.requireCross) {
                            self.retrieveUnassignedCrosses();
                            return false;
                        }
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Requirement Retrieval Failed!",
                        message: "Could not retrieve requirements" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.retrieveUnassignedRings = function () {
                var self = _this;
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
                    success: function (data) {
                        $.each(data, function (i, val) {
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
            };
            this.retrieveUnassignedCrosses = function () {
                var self = _this;
                self.dataModel.crossOptions.removeAll();
                $.ajax({
                    method: 'get',
                    url: warriorDashboardViewModel.crossUrl + '/unassigned',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data, function (i, val) {
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
            };
            this.retrieveApprovalStatus = function (rank) {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: warriorDashboardViewModel.rankStatusUrl + '/approvalsForRank/' + rank.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        data = data.sort(function (a, b) { return b.warriorCompletedTs.localeCompare(a.warriorCompletedTs); });
                        var pendingApproval = data.filter(function (a) { return a.returnedTs == null && a.guardianApprovedTs == null; });
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
                        var rejectedApproval = data[0];
                        if (rejectedApproval === null || rejectedApproval === void 0 ? void 0 : rejectedApproval.returnedTs) {
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
            };
            this.markAsComplete = function (data) {
                if (!warriorDashboardViewModel.readOnly) {
                    if (!data.requireAttachment()) {
                        var self = _this;
                        var validationMessages = self.validateMarkAsCompleteRequest(data);
                        if (validationMessages.length === 0) {
                            //var selectedRings = data.selectedRings().map(ring => ring.ringId);
                            //var selectedCrosses = data.selectedCrosses().map(cross => cross.crossId);
                            _this.rankService.markRequirementComplete(data.rankId, data.id, function (rank) {
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
                            }, function (err) {
                                var errFromServer = WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err);
                                BootstrapAlert.alert({
                                    title: "Action Failed!",
                                    message: "Could not mark the given requirement as complete" + errFromServer
                                });
                            });
                        }
                        else {
                            BootstrapAlert.alert({
                                title: "Action Failed!",
                                message: "Request is not valid. " + validationMessages.join('. ') + '.'
                            });
                        }
                    }
                }
            };
            this.validateMarkAsCompleteRequest = function (data) {
                return [];
                var messages = new Array(0);
                if (data.requireRing()) {
                    if (data.selectedRings().length === data.requiredRingCount()) {
                        var selectedRings = new Array(0);
                        $.each(data.selectedRings(), function (i, val) {
                            if (val.ringId === null || val.ringId === undefined || val.ringId.length !== 36) {
                                messages.push('One or more ring(s) does not have a valid selection');
                                return false;
                            }
                            else {
                                if ($.grep(selectedRings, function (el, i) {
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
                        $.each(data.selectedCrosses(), function (i, val) {
                            if (val.crossId === null || val.crossId === undefined || val.crossId.length !== 36) {
                                messages.push('One or more cross(es) does not have a valid selection');
                                return false;
                            }
                            else {
                                if ($.grep(selectedCrosses, function (el, i) {
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
            };
            this.downloadProofOfCompletionFile = function (data) {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: warriorDashboardViewModel.proofOfCompletionUrl + '/OneUseFileKey/' + data.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (oneTimeAccessToken) {
                        window.open("".concat(warriorDashboardViewModel.proofOfCompletionUrl, "/").concat(oneTimeAccessToken));
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.revertCompletion = function (data) {
                var self = _this;
                $.ajax({
                    method: 'delete',
                    url: warriorDashboardViewModel.rankStatusUrl,
                    data: ko.toJSON({ RankRequirementId: data.id, RankId: data.rankId }),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (rank) {
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
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not mark the given requirement as incomplete"
                        });
                    }
                });
            };
            this.rankForPromotion = null;
            this.beginRequestForPromotion = function (data) {
                _this.rankForPromotion = data;
                $('#requestPromotionPopup').modal('show');
            };
            this.submitForPromotion = function () {
                var self = _this;
                $('#requestPromotionPopup').modal('hide');
                $.ajax({
                    method: 'post',
                    url: warriorDashboardViewModel.rankStatusUrl + '/SubmitForApproval/' + self.rankForPromotion.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (rank) {
                        BootstrapAlert.success({
                            title: "Action Success!",
                            message: "Rank submitted for promotion"
                        });
                        self.getWorkingAndCompletedRank();
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not submit this Rank for promotion"
                        });
                    }
                });
            };
            this.recallRequestForPromotion = function (data) {
                var self = _this;
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
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The rank could not be recalled" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.returnCross = function (data) {
                var self = _this;
                if (!data.warriorCompleted())
                    return;
                $.ajax({
                    method: 'post',
                    url: "".concat(warriorDashboardViewModel.crossStatusUrl).concat(data.id(), "/return?reason=test"),
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
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The cross could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.CreateObservableRankRequirement = function (rankId, data) {
                var _a;
                var self = _this;
                var result = new WarriorsGuild.ObservableRankRequirement();
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
                    data.crossesToComplete.forEach(function (c) {
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
                result.visible((_a = data.visible) !== null && _a !== void 0 ? _a : true);
                result.initiatedByGuardian(data.initiatedByGuardian);
                result.showAtPercent(data.showAtPercent);
                if (data.requiredRingCount != null) {
                    for (var i = 0; i < data.requiredRingCount; i++) {
                        result.selectedRings.push(new WarriorsGuild.MinimumRingDetail());
                    }
                }
                if (data.requiredCrossCount != null) {
                    for (var i = 0; i < data.requiredCrossCount; i++) {
                        result.selectedCrosses.push(new WarriorsGuild.MinimumCrossDetail());
                    }
                }
                $.each(data.savedRings, function (i, r) {
                    result.savedRings.push(r);
                });
                $.each(data.savedCrosses, function (i, r) {
                    result.savedCrosses.push(r);
                });
                $.each(data.attachments, function (i, r) {
                    result.attachments.push(ko.mapping.fromJS(r));
                });
                return result;
            };
            this.calculateSubRank = function (percentComplete) {
                if (percentComplete === 100) {
                    return 'Master';
                }
                else if (percentComplete >= 66) {
                    return 'Journeyman';
                }
                else if (percentComplete >= 33) {
                    return 'Apprentice';
                }
            };
            this.showPopup = function (popupId, warriorId) {
                $.each($(".popuptext:not(#".concat(popupId).concat(warriorId, ")")), function (index, el) {
                    el.classList.add('hidden');
                });
                var popupElement = document.getElementById(popupId + warriorId);
                popupElement ? popupElement.classList.add("hidden") : null;
                document.getElementsByClassName("ui-widget-overlay")[0].classList.add("hidden");
            };
            this.closePopup = function (popupId, warriorId) {
                var popupElement = document.getElementById(popupId);
                popupElement.classList.add("hidden");
                document.getElementsByClassName("ui-widget-overlay")[0].classList.add("hidden");
            };
            var self = this;
            self.app = app;
            this.rankService = new WarriorsGuild.RankService();
            this.dataModel = {
                BlogEntries: ko.observableArray([]),
                CompletedRank: ko.observable(new WarriorsGuild.ObservableRank()),
                WorkingRank: ko.observable(new WarriorsGuild.ObservableRank()),
                CompletedCompletionPercentage: ko.observable(0),
                WorkingCompletionPercentage: ko.observable(0),
                PinnedRings: ko.observableArray([]),
                CompletedSilverRings: ko.observableArray([]),
                CompletedGoldRings: ko.observableArray([]),
                CompletedPlatinumRings: ko.observableArray([]),
                PinnedCrosses: ko.observableArray([]),
                CompletedCrosses: ko.observableArray([]),
                PendingRankApproval: ko.observable(),
                OnLockDown: ko.pureComputed(function () { return true; }),
                silverRingOptions: ko.observableArray(),
                goldRingOptions: ko.observableArray(),
                platinumRingOptions: ko.observableArray(),
                crossOptions: ko.observableArray(),
                crossesToComplete: ko.observableArray(),
                PendingCrossApprovals: ko.observableArray([])
            };
            self.dataModel.OnLockDown = ko.pureComputed(function () {
                return this.PendingRankApproval() != null;
            }, self.dataModel);
            self.RetrievingRank = ko.observable(false);
            self.RetrievingPinnedRings = ko.observable(false);
            self.RetrievingPinnedCrosses = ko.observable(false);
            self.RetrievingPinnedRingsError = ko.observable(false);
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
                    var currContext = this;
                    self.profileId(currContext.params.id);
                    this.app.runRoute('get', '#home');
                });
                this.get('/Profile/:id', function () {
                    var currContext = this;
                    self.profileId(currContext.params.id);
                    this.app.runRoute('get', '#home');
                });
                this.get('/Dashboard/Warrior', function () { this.app.runRoute('get', '#home'); });
                this.get('/dashboard/warrior', function () { this.app.runRoute('get', '#home'); });
                this.get('/Dashboard', function () { this.app.runRoute('get', '#home'); });
            });
        }
        WarriorDashboardViewModel.prototype.nullableDate = function (dateString) {
            return dateString === null ? null : new Date(dateString);
        };
        return WarriorDashboardViewModel;
    }());
    WarriorsGuild.WarriorDashboardViewModel = WarriorDashboardViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Dashboard",
    bindingMemberName: "dashboard",
    factory: WarriorsGuild.WarriorDashboardViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=warriorDashboard.viewmodel.js.map