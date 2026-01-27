var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
var WarriorsGuild;
(function (WarriorsGuild) {
    var RankViewModel = /** @class */ (function () {
        function RankViewModel(app) {
            var _this = this;
            this.ranksUrl = '/api/ranks';
            this.koRankMapperConfiguration = (function () {
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
            this.retrieveRequirements = function (rankId) {
                var self = _this;
                _this.rankService.retrieveRankRequirements(rankId, function (data) {
                    self.rankDetail.requirements.removeAll();
                    ko.mapping.fromJS(data, {
                        create: function (options) {
                            var req = options.data;
                            if (req.requireCross) {
                                for (var _i = 0, _a = req.crossesToComplete; _i < _a.length; _i++) {
                                    var c = _a[_i];
                                    self.dataModel.crossesToComplete.push(c);
                                }
                            }
                            return self.CreateObservableRequirement(rankId, options.data);
                        }
                    }, self.rankDetail.requirements);
                    if (isLoggedIn) {
                        //$.each(data, (i, el): boolean => {
                        //	if (el.requireRing) {
                        //		self.retrieveUnassignedRings();
                        //		return false;
                        //	}
                        //});
                        //$.each(data, (i, el): boolean => {
                        //	if (el.requireCross) {
                        //		self.retrieveUnassignedCrosses();
                        //		return false;
                        //	}
                        //});
                    }
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Requirement Retrieval Failed!",
                        message: "Could not retrieve requirements" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                });
            };
            this.retrieveUnassignedRings = function () {
                var self = _this;
                self.dataModel.silverRingOptions.removeAll();
                self.dataModel.goldRingOptions.removeAll();
                self.dataModel.platinumRingOptions.removeAll();
                $.ajax({
                    method: 'get',
                    url: rankUrls.ringStatusUrl + '/unassigned',
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
                            message: "Could not retrieve unassigned rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
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
                    url: rankUrls.crossUrl + '/unassigned',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data, function (i, val) {
                            var g = new WarriorsGuild.MinimumCrossDetail();
                            g = __assign(__assign({}, g), val);
                            self.dataModel.crossOptions.push(g);
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Unassigned Crosses Retrieval Failed!",
                            message: "Could not retrieve unassigned crosses" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.retrieveApprovalStatus = function (rankId) {
                var self = _this;
                _this.rankService.getPendingApprovals(rankId, function (data) {
                    data = data.sort(function (a, b) { return b.warriorCompletedTs.localeCompare(a.warriorCompletedTs); });
                    var pendingApproval = data.filter(function (a) { return a.returnedTs == null && a.guardianApprovedTs == null; });
                    if (pendingApproval.length) {
                        self.rankDetail.highestPercentSubmittedForApproval(pendingApproval[0].percentComplete);
                        self.rankDetail.approvalRecordId(pendingApproval[0].approvalRecordId);
                        self.rankDetail.completedTs(self.nullableDate(pendingApproval[0].warriorCompletedTs));
                        self.rankDetail.guardianApprovedTs(self.nullableDate(pendingApproval[0].guardianApprovedTs));
                    }
                    else {
                        self.rankDetail.highestPercentSubmittedForApproval(0);
                        self.rankDetail.approvalRecordId(null);
                        self.rankDetail.completedTs(null);
                        self.rankDetail.guardianApprovedTs(null);
                    }
                    var rejectedApproval = data[0];
                    if (rejectedApproval === null || rejectedApproval === void 0 ? void 0 : rejectedApproval.returnedTs) {
                        self.rankDetail.returnReason(rejectedApproval.returnReason);
                    }
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Rank Approval Retrieval Failed!",
                        message: "Could not retrieve rank approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                    //self.actionFailureMessage( 'The Ring could not be saved' );
                });
            };
            this.saveNewRankOrder = function (data) {
                var self = _this;
                var rankOrder = [];
                $.each(self.dataModel.Ranks(), function (index, value) {
                    var objToSave = { Id: value.id(), Index: index + 1 };
                    rankOrder.push(objToSave);
                });
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                _this.rankService.saveNewRankOrder(rankOrder, function () {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "Rank Order has been updated"
                    });
                }, function (err) {
                    data.cancelDrop = true;
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "Rank order could not be updated" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.saveRank = function () {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                _this.rankService.create({ Name: _this.dataModel.Name(), Description: _this.dataModel.Description() }, function (data) {
                    var oRank = ko.mapping.fromJS(data, {}, new WarriorsGuild.ObservableRank());
                    self.dataModel.Ranks.push(oRank);
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The Rank has been saved"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The Rank save process failed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.updateRank = function (data, saveUrl) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var requirementsToSave = [];
                $.each(data.requirements(), function (index, value) {
                    var objToSave = {
                        RankId: data.id(),
                        ActionToComplete: value.actionToComplete(),
                        Weight: value.weight(),
                        ImageUploaded: data.imageUploaded(),
                        Index: index,
                        CrossesToComplete: value.crossesToComplete().map(function (c) { return ({ id: c.crossId() }); })
                    };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    requirementsToSave.push(objToSave);
                });
                _this.rankService.update({ Id: data.id(), Name: data.name(), Description: data.description(), Requirements: requirementsToSave }, function () {
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "The Rank has been saved"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The Rank could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.updateRequirements = function () {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var requirementsToSave = [];
                var cancelUpdate = false;
                $.each(self.rankDetail.requirements(), function (index, value) {
                    var objToSave = {
                        RankId: self.rankDetail.id(),
                        ActionToComplete: value.actionToComplete(),
                        Weight: value.weight(),
                        RequireAttachment: value.requireAttachment(),
                        RequireRing: value.requireRing(),
                        RequiredRingType: value.requireRing() ? value.requiredRingType() : null,
                        RequiredRingCount: value.requireRing() ? value.requiredRingCount() : null,
                        RequireCross: value.requireCross(),
                        RequiredCrossCount: value.requireCross() ? value.requiredCrossCount() : null,
                        Index: index,
                        SeeHowLink: value.seeHowLink(),
                        Optional: value.optional(),
                        CrossesToComplete: value.crossesToComplete().map(function (c) { return ({ id: c.crossId }); }),
                        InitiatedByGuardian: value.initiatedByGuardian,
                        ShowAtPercent: value.showAtPercent
                    };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    requirementsToSave.push(objToSave);
                    if (value.seeHowLinkInvalid()) {
                        cancelUpdate = true;
                        BootstrapAlert.alert({
                            title: "Invalid See How link",
                            message: "Correct invalid See How link to save"
                        });
                        return;
                    }
                });
                if (cancelUpdate)
                    return;
                $.ajax({
                    method: 'put',
                    url: _this.ranksUrl + '/' + self.rankDetail.id() + '/requirements',
                    data: ko.toJSON(requirementsToSave),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "The Rank has been saved"
                        });
                    },
                    error: function (err) {
                        var responseMsg = "";
                        if (!err.responseText ? '' : ". ".concat(err.responseText) > '') {
                            responseMsg += !err.responseText ? '' : ". ".concat(err.responseText);
                        }
                        if (err.responseJSON) {
                            if (err.responseJSON.message) {
                                responseMsg += err.responseJSON.message;
                            }
                            for (var key in err.responseJSON) {
                                responseMsg += "".concat(key, ": ").concat(err.responseJSON[key]);
                            }
                        }
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The Rank could not be saved" + (responseMsg > "" ? '. ' + responseMsg : '')
                        });
                    }
                });
            };
            this.markAsComplete = function (data, event) {
                var self = _this;
                if (!data.requireAttachment()) {
                    var validationMessages = self.validateMarkAsCompleteRequest(data);
                    if (validationMessages.length === 0) {
                        //var selectedRings = data.selectedRings().map(ring => ring.ringId);
                        //var selectedCrosses = data.selectedCrosses().map(cross => cross.crossId);
                        $.ajax({
                            method: 'post',
                            url: rankUrls.recordCompletion,
                            data: ko.toJSON({ RankRequirementId: data.id, RankId: data.rankId }),
                            contentType: "application/json; charset=utf-8",
                            headers: {
                                'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                            },
                            success: function (rank) {
                                BootstrapAlert.success({
                                    title: "Action Success!",
                                    message: "The requirement has been marked as complete"
                                });
                                data.savedCrosses(data.selectedCrosses().map(function (c) { return ko.utils.arrayFirst(self.dataModel.crossOptions(), function (co) { return co.crossId == c.crossId; }); }));
                                data.savedRings(data.selectedRings().map(function (val) {
                                    switch (data.requiredRingType()) {
                                        case "Silver":
                                            return ko.utils.arrayFirst(self.dataModel.silverRingOptions(), function (ro) { return ro.ringId == val.ringId; });
                                        case "Gold":
                                            return ko.utils.arrayFirst(self.dataModel.goldRingOptions(), function (ro) { return ro.ringId == val.ringId; });
                                        case "Platinum":
                                            return ko.utils.arrayFirst(self.dataModel.platinumRingOptions(), function (ro) { return ro.ringId == val.ringId; });
                                    }
                                }));
                                if (!data.warriorCompleted()) {
                                    data.warriorCompletedTs(new Date());
                                }
                                else {
                                    data.guardianReviewedTs(new Date());
                                }
                                //self.dataModel.crossOptions.remove(i => $.grep(selectedCrosses, e => { return e === i.crossId; }).length > 0);
                                //$.each(selectedRings, (i, val) => {
                                //	self.dataModel.silverRingOptions.remove(i => i.ringId === val);
                                //	self.dataModel.goldRingOptions.remove(i => i.ringId === val);
                                //	self.dataModel.platinumRingOptions.remove(i => i.ringId === val);
                                //});
                            },
                            error: function (err) {
                                BootstrapAlert.alert({
                                    title: "Action Failed!",
                                    message: "Could not mark the given requirement as complete" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                                });
                            }
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Request is not valid. " + validationMessages.join('. ') + '.'
                        });
                    }
                }
            };
            this.validateMarkAsCompleteRequest = function (data) {
                var messages = new Array(0);
                if (data.requireRing()) {
                    if (data.selectedRings().length === data.requiredRingCount()) {
                        var selectedCrosses = new Array(0);
                        $.each(data.selectedRings(), function (i, val) {
                            if (val.ringId === null || val.ringId === undefined || val.ringId.length !== 36) {
                                messages.push('One or more ring(s) does not have a valid selection');
                                return false;
                            }
                            else {
                                if ($.grep(selectedCrosses, function (el, i) {
                                    return el === val.ringId;
                                }).length === 0) {
                                    selectedCrosses.push(val.ringId);
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
            this.revertCompletion = function (data, saveUrl) {
                var self = _this;
                $.ajax({
                    method: 'delete',
                    url: rankUrls.rankStatusUrl,
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
                            message: "Could not mark the given requirement as incomplete" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.completionPercentUpdatedHandler = function (data, percentComplete) {
                var requirementCompletion = data.weight() * percentComplete / 100;
                data.totalPercentComplete(requirementCompletion);
            };
            this.beginRequestForPromotion = function () {
                $('#requestPromotionPopup').modal('show');
            };
            this.submitForPromotion = function (data) {
                var self = _this;
                $('#requestPromotionPopup').modal('hide');
                $.ajax({
                    method: 'post',
                    url: rankUrls.rankStatusUrl + '/SubmitForApproval/' + data.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (rank) {
                        BootstrapAlert.success({
                            title: "Action Success!",
                            message: "Rank submitted for promotion"
                        });
                        self.retrieveApprovalStatus(data.id());
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not submit this Rank for promotion" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.recallRequestForPromotion = function (data) {
                var self = _this;
                if (!data.warriorCompleted())
                    return;
                $.ajax({
                    method: 'post',
                    url: rankUrls.rankStatusUrl + '/' + data.approvalRecordId() + '/return',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        self.retrieveApprovalStatus(data.id());
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
            this.rankToReturn = null;
            this.beginReturn = function (data) {
                $('#returnReasonDialog').modal('show');
                _this.rankToReturn = data;
            };
            this.cancelReturn = function () {
                _this.dataModel.reasonForReturn('');
            };
            this.returnToWarrior = function () {
                var data = _this.rankToReturn;
                var self = _this;
                if (!data.warriorCompleted())
                    return;
                _this.rankService.returnToWarrior(data.approvalRecordId(), self.dataModel.reasonForReturn(), function () {
                    self.retrieveApprovalStatus(data.id());
                    BootstrapAlert.success({
                        title: "Save Success!",
                        message: "This rank has been returned"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Save Failed!",
                        message: "The rank could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.confirmRankCompletion = function () {
                var self = _this;
                _this.rankService.confirmRankCompletion(self.rankDetail.approvalRecordId(), function () {
                    self.retrieveApprovalStatus(self.rankDetail.id());
                    BootstrapAlert.success({
                        title: "Action Success!",
                        message: "The Warriors rank progress has been confirmed"
                    });
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Action Failed!",
                        message: "Could not confirm the rank completion" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.downloadProofOfCompletionFile = function (data) {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: rankUrls.proofOfCompletionUrl + '/OneUseFileKey/' + data.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (oneTimeAccessToken) {
                        window.open("".concat(rankUrls.proofOfCompletionUrl, "/").concat(oneTimeAccessToken));
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.CreateObservableRequirement = function (rankId, data) {
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
                result.requireAttachment(data.requireAttachment && isLoggedIn);
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
                data.savedRings.forEach(function (r) {
                    result.savedRings.push(r);
                });
                if (data.crossesToComplete.length > 0) {
                    result.crossesToComplete.removeAll();
                    var totalComplete = 0;
                    $.each(data.crossesToComplete, function (i, r) {
                        totalComplete += r.percentCompleted;
                        result.crossesToComplete.push(r);
                    });
                    var requirementCompletion = data.weight * totalComplete / 100;
                    result.totalPercentComplete(requirementCompletion);
                }
                $.each(data.savedCrosses, function (i, r) {
                    result.savedCrosses.push(r);
                });
                $.each(data.attachments, function (i, r) {
                    result.attachments.push(ko.mapping.fromJS(r));
                });
                return result;
            };
            this.closePopup = function (popupId, warriorId) {
                var popupElement = document.getElementById(popupId);
                popupElement.classList.add("hidden");
                document.getElementsByClassName("ui-widget-overlay")[0].classList.add("hidden");
            };
            this.app = app;
            var self = this;
            this.rankService = new WarriorsGuild.RankService();
            this.dataModel = {
                Ranks: ko.observableArray(),
                Name: ko.observable(''),
                Description: ko.observable(''),
                selectedWarrior: app.dataModel.selectedWarrior,
                ringTypes: ringTypes,
                silverRingOptions: ko.observableArray(),
                goldRingOptions: ko.observableArray(),
                platinumRingOptions: ko.observableArray(),
                crossOptions: ko.observableArray(),
                reasonForReturn: ko.observable(''),
                crossesToComplete: ko.observableArray(),
                PendingCrossApprovals: ko.observableArray([]),
                CrossDays: ko.observableArray([])
            };
            this.DetailView = ko.observable(false);
            this.rankDetail = new WarriorsGuild.ObservableRank();
            this.PreviewMode = ko.observable(previewMode);
            this.ReturnToListView = function () {
                self.DetailView(false);
            };
            app.prepareAjax(false);
            Sammy(function () {
                this.get('#ranks', function () {
                    // Make a call to the protected Web API by passing in a Bearer Authorization Header
                    if (canViewAll) {
                        self.rankService.getRanks(function (data) {
                            self.dataModel.Ranks.removeAll();
                            $.each(data, function (i, d) {
                                var oRank = ko.mapping.fromJS(d, self.koRankMapperConfiguration, new WarriorsGuild.ObservableRank());
                                self.dataModel.Ranks.push(oRank);
                            });
                        }, function (err) {
                            BootstrapAlert.alert({
                                title: "Retrieve Failed!",
                                message: "Could not get Rank list" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                            });
                        });
                        self.DetailView(false);
                    }
                    else {
                        self.rankService.getPublicRank(function (data) {
                            self.rankDetail.requirements.removeAll();
                            ko.mapping.fromJS(data, self.koRankMapperConfiguration, self.rankDetail);
                            if (data != null) {
                                self.DetailView(true);
                                self.retrieveRequirements(data.id);
                            }
                        }, function (err) {
                            BootstrapAlert.alert({
                                title: "Retrieve Failed!",
                                message: "Could not get Rank detail" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                            });
                        });
                    }
                });
                this.post('', function () { return true; }); //for image upload
                this.get('/ranks', function () { this.app.runRoute('get', '#ranks'); });
                this.get('/Ranks', function () { this.app.runRoute('get', '#ranks'); });
                this.get('/Ranks#detail', function () { this.app.runRoute('get', '/ranks#detail', this.params); });
                this.get('/ranks#detail', function () {
                    var currContext = this;
                    if (self.dataModel.Ranks().length === 0) {
                        self.rankService.getRankDetail(currContext.params.id, function (data) {
                            self.rankDetail.requirements.removeAll();
                            ko.mapping.fromJS(data, self.koRankMapperConfiguration, self.rankDetail);
                            self.retrieveRequirements(currContext.params.id);
                            self.retrieveApprovalStatus(currContext.params.id);
                        }, function (err) {
                            BootstrapAlert.alert({
                                title: "Retrieve Failed!",
                                message: "Could not get Rank detail" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                            });
                        });
                    }
                    else {
                        self.rankDetail.requirements.removeAll();
                        $.each(self.dataModel.Ranks(), function (i, e) {
                            if (e.id() === currContext.params.id) {
                                var rankToEdit = ko.mapping.toJS(e);
                                ko.mapping.fromJS(rankToEdit, self.koRankMapperConfiguration, self.rankDetail);
                                self.retrieveRequirements(currContext.params.id);
                                self.retrieveApprovalStatus(currContext.params.id);
                                return false;
                            }
                        });
                    }
                    self.retrieveUnassignedCrosses();
                    self.DetailView(true);
                });
            });
        }
        RankViewModel.prototype.nullableDate = function (dateString) {
            return dateString === null ? null : new Date(dateString);
        };
        return RankViewModel;
    }());
    WarriorsGuild.RankViewModel = RankViewModel;
    var RanksDataModel = /** @class */ (function () {
        function RanksDataModel() {
        }
        return RanksDataModel;
    }());
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Ranks",
    bindingMemberName: "ranks",
    factory: WarriorsGuild.RankViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=ranks.viewmodel.js.map