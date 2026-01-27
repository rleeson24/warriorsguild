var WarriorsGuild;
(function (WarriorsGuild) {
    var GuardianDashboardViewModel = /** @class */ (function () {
        function GuardianDashboardViewModel(app, dataModel) {
            var _this = this;
            this.GetProfileData = function (warrior) {
                var self = _this;
                warrior.RetrievingRank(true);
                //Make a call to the protected Web API by passing in a Bearer Authorization Header
                _this.getWorkingAndCompletedRanks(warrior);
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.ringsByUserUrl + warrior.id + '/pinned',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data, function (i, r) {
                            var m = new WarriorsGuild.MinimumRingDetail();
                            m.hasImage = r.imageUploaded !== null;
                            m.ringId = r.id;
                            m.imageExtension = r.imageExtension;
                            m.name = r.name;
                            m.imgSrcAttr = m.hasImage ? "/images/rings/" + r.id + r.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
                            warrior.PinnedRings.push(m);
                        });
                    },
                    error: function (err) {
                        warrior.RetrievingPinnedRings(false);
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving Pinned Rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.crossesByUserUrl + warrior.id + '/pinned',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data, function (i, c) {
                            var m = new WarriorsGuild.MinimumCrossDetail();
                            m.hasImage = c.imageUploaded != null;
                            m.crossId(c.id);
                            m.imageExtension = c.imageExtension;
                            m.name = c.name;
                            m.imgSrcAttr = m.hasImage ? "/images/rings/" + c.id + c.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
                            warrior.PinnedCrosses.push(m);
                        });
                    },
                    error: function (err) {
                        warrior.RetrievingPinnedRings(false);
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving Pinned Crosses" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.ringsByUserUrl + warrior.id + '/completed',
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
                                    warrior.CompletedSilverRings.push(oRing);
                                    break;
                                case gold:
                                    warrior.CompletedGoldRings.push(oRing);
                                    break;
                                case platinum:
                                    warrior.CompletedPlatinumRings.push(oRing);
                                    break;
                            }
                        });
                    },
                    error: function (err) {
                        warrior.RetrievingPinnedRings(false);
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving completed rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.crossesByUserUrl + warrior.id + '/completed',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data.sort(function (a, b) { return a.name.localeCompare(b.name); }), function (i, v) {
                            var oCross = ko.mapping.fromJS(v, self.koMapperConfiguration, new WarriorsGuild.ObservableCross());
                            warrior.CompletedCrosses.push(oCross);
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving completed crosses" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.getWorkingAndCompletedRanks = function (warrior) {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.ranksByUserUrl + warrior.id,
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        warrior.RetrievingRank(false);
                        warrior.CompletedRank().requirements.removeAll();
                        if (data.completedRank !== null) {
                            ko.mapping.fromJS(data.completedRank, self.koMapperConfiguration, warrior.CompletedRank);
                            warrior.CompletedRank().subRank(self.calculateSubRank(data.completedRank.percentComplete));
                        }
                        warrior.WorkingRank().requirements.removeAll();
                        ko.mapping.fromJS(data.workingRank, self.koMapperConfiguration, warrior.WorkingRank);
                        self.retrieveRequirements(warrior.WorkingRank());
                        warrior.WorkingCompletionPercentage(data.workingCompletionPercentage);
                        warrior.CompletedCompletionPercentage(data.completedCompletionPercentage);
                    },
                    error: function (err) {
                        warrior.RetrievingRank(false);
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred attempting to retrieve your rank" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.GetPendingApprovals = function (warrior) {
                var self = _this;
                warrior.RetrievingPendingApproval(true);
                warrior.RetrievingPinnedRings(true);
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.rankPendingApprovalUrl + warrior.id,
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        warrior.RetrievingRank(false);
                        if (data != null) {
                            data.hasImage = data.rankImageUploaded != null;
                            data.pendingSubRank = self.calculateSubRank(data.percentComplete);
                            data.imgSrcAttr = data.hasImage ? '/images/ranks/' + data.rankId + data.imageExtension : '/images/logo/Warriors-Guild-icon-sm-wide.png';
                            $.each(data.unconfirmedRequirements, function (i, r) {
                                var _a;
                                r.savedRings = self.MapToMinimumRingDetail(r.savedRings);
                                r.savedCrosses = self.MapToMinimumCrossDetail(r.savedCrosses);
                                r.actionToCompleteLinked = (_a = r.actionToComplete) === null || _a === void 0 ? void 0 : _a.replace(/\n/g, '<br />').replace(/\[link ([a-zA-Z ]+)\]/g, "<a href='".concat(r.seeHowLink, "' target='_blank'>$1</a>"));
                            });
                        }
                        warrior.PendingRankApproval(data);
                        warrior.RetrievingPendingApproval(false);
                    },
                    error: function (err) {
                        warrior.RetrievingPendingApproval(false);
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred attempting to retrieve rank approval" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.ringsPendingApprovalUrl + warrior.id,
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        //warrior.RetrievingRing( false );
                        $.each(data, function (index, d) {
                            d.hasImage = d.ringImageUploaded != null;
                            d.imgSrcAttr = d.hasImage ? '/images/rings/' + d.ringId + d.imageExtension : '/images/logo/Warriors-Guild-icon-sm-wide.png';
                        });
                        warrior.PendingRingApprovals(data);
                        warrior.RetrievingPinnedRings(false);
                    },
                    error: function (err) {
                        warrior.RetrievingPinnedRings(false);
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred attempting to retrieve the pending ring approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.crossesPendingApprovalUrl + warrior.id,
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        //warrior.RetrievingRing( false );
                        $.each(data, function (index, d) {
                            d.hasImage = d.crossImageUploaded != null;
                            d.imgSrcAttr = d.hasImage ? '/images/crosses/' + d.crossId + d.imageExtension + '?' : '/images/logo/Warriors-Guild-icon-sm-wide.png';
                        });
                        warrior.PendingCrossApprovals(data);
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred attempting to retrieve the pending cross approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.approvalRecordId = null;
            this.warrior = null;
            this.beginReturn = function (approvalRecordId, warrior) {
                $('#returnReasonDialog').modal('show');
                _this.approvalRecordId = approvalRecordId;
                _this.warrior = warrior;
            };
            this.cancelReturn = function () {
                _this.dataModel.reasonForReturn('');
            };
            this.returnRank = function () {
                var approvalRecordId = _this.approvalRecordId;
                var warrior = _this.warrior;
                var self = _this;
                $.ajax({
                    method: 'post',
                    url: "".concat(guardianDashboardViewModel.rankStatusUrl, "/").concat(approvalRecordId, "/return?reason=").concat(self.dataModel.reasonForReturn()),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        warrior.PendingRankApproval(null);
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "This rank has been returned"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The rank could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.promoteWarrior = function (approvalRecordId, warrior) {
                var self = _this;
                $.ajax({
                    method: 'post',
                    url: guardianDashboardViewModel.rankStatusUrl + '/' + approvalRecordId + '/ApproveProgress',
                    data: ko.toJSON({ ApprovalRecordId: approvalRecordId, UserId: warrior.id }),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (rank) {
                        BootstrapAlert.success({
                            title: "Action Success!",
                            message: "The Warrior has been Promoted"
                        });
                        self.getWorkingAndCompletedRanks(warrior);
                        warrior.PendingRankApproval(null);
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not Promote the Warrior" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
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
            this.setCurrentAndWorkingRanks = function (warrior, data) {
                var self = _this;
                warrior.CompletedRank().requirements.removeAll();
                ko.mapping.fromJS(data.completedRank, self.koMapperConfiguration, warrior.CompletedRank);
                ko.mapping.fromJS(data.workingRank, self.koMapperConfiguration, warrior.WorkingRank);
                self.retrieveRequirements(warrior.WorkingRank());
                warrior.WorkingCompletionPercentage(data.workingCompletionPercentage);
                warrior.CompletedCompletionPercentage(data.completedCompletionPercentage);
            };
            this.retrieveRequirements = function (rank) {
                var self = _this;
                _this.rankService.retrieveRankRequirements(rank.id(), function (data) {
                    rank.requirements.removeAll();
                    ko.mapping.fromJS(data, {
                        create: function (options) {
                            return self.CreateObservableRankRequirement(rank.id(), options.data);
                        }
                    }, rank.requirements);
                }, function (err) {
                    BootstrapAlert.alert({
                        title: "Requirement Retrieval Failed!",
                        message: "Could not retrieve requirements" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                });
            };
            this.markAsComplete = function (data, saveUrl) {
                if (!guardianDashboardViewModel.readOnly) {
                    var self = _this;
                    self.rankService.markRequirementCompleteByGuardian(data.rankId, data.id, function (rank) {
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
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not mark the given requirement as complete" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    });
                }
            };
            this.revertCompletion = function (data, saveUrl) {
                if (!guardianDashboardViewModel.readOnly) {
                    var self = _this;
                    $.ajax({
                        method: 'post',
                        url: guardianDashboardViewModel.markAsComplete,
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
                            if (!data.warriorCompleted()) {
                                data.warriorCompletedTs(new Date());
                            }
                            else {
                                data.guardianReviewedTs(new Date());
                            }
                        },
                        error: function (err) {
                            BootstrapAlert.alert({
                                title: "Action Failed!",
                                message: "Could not mark the given requirement as complete" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                            });
                        }
                    });
                }
            };
            this.downloadProofOfCompletionFile = function (data) {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: guardianDashboardViewModel.proofOfCompletionUrl + '/OneUseFileKey/' + data.id,
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (oneTimeAccessToken) {
                        window.open("".concat(guardianDashboardViewModel.proofOfCompletionUrl, "/").concat(oneTimeAccessToken));
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
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
            this.CreateObservableRingRequirement = function (rankOrRing, data) {
                var self = _this;
                var result = new WarriorsGuild.ObservableRingRequirement();
                result.id = data.id;
                result.ringId = rankOrRing.id;
                result.weight(data.weight);
                result.actionToComplete(data.actionToComplete);
                result.markAsComplete = self.markAsComplete;
                $.each(rankOrRing.statuses, function (i, s) {
                    if (s.ringRequirementId === data.id) {
                        result.warriorCompletedTs(new Date(s.warriorCompleted));
                        result.guardianReviewedTs(s.guardianCompleted !== null ? new Date(s.guardianCompleted) : null);
                    }
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
            this.MapToMinimumRingDetail = function (rings) {
                var result = [];
                $.each(rings, function (i, r) {
                    var m = new WarriorsGuild.MinimumRingDetail();
                    m.hasImage = r.hasImage;
                    m.ringId = r.ringId;
                    m.imageExtension = r.imageExtension;
                    m.name = r.name;
                    m.imgSrcAttr = r.imgSrcAttr;
                    result.push(m);
                });
                return result;
            };
            this.MapToMinimumCrossDetail = function (rings) {
                var result = [];
                $.each(rings, function (i, r) {
                    var m = new WarriorsGuild.MinimumCrossDetail();
                    m.hasImage = r.hasImage;
                    m.crossId = r.crossId;
                    m.imageExtension = r.imageExtension;
                    m.name = r.name;
                    m.imgSrcAttr = r.imgSrcAttr;
                    result.push(m);
                });
                return result;
            };
            this.showPopup = function (popupId, warriorId) {
                $.each($(".popuptext:not(#".concat(popupId).concat(warriorId, ")")), function (index, el) {
                    el.classList.remove('show');
                });
                var popupElement = document.getElementById(popupId + warriorId);
                popupElement ? popupElement.classList.toggle("show") : null;
            };
            this.closePopup = function (popupId, warriorId) {
                var popupElement = document.getElementById(popupId + warriorId);
                popupElement.classList.toggle("show");
            };
            var self = this;
            self.app = app;
            this.rankService = new WarriorsGuild.RankService();
            this.dataModel = {
                BlogEntries: ko.observableArray([]),
                Warriors: ko.observableArray(),
                guardian: new ObservableGuardian(),
                PinnedRings: ko.observableArray([]),
                reasonForReturn: ko.observable('')
            };
            self.dataModel.BlogEntries = ko.observableArray([]);
            app.prepareAjax();
            self.RetrievingPinnedRings = ko.observable(false);
            Sammy(function () {
                this.get('#home', function () {
                    self.RetrievingPinnedRings(true);
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
                        url: guardianDashboardViewModel.warriorListUrl,
                        contentType: "application/json; charset=utf-8",
                        headers: {
                            'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                        },
                        success: function (data) {
                            $.each(data, function (i, v) {
                                var w = new ObservableWarrior();
                                w.id = v.id;
                                w.name = v.name;
                                w.username = v.username;
                                w.avatarSrc = v.avatarSrc;
                                w.HasAvatar = v.avatarSrc.length > 0;
                                self.dataModel.Warriors.push(w);
                                self.GetProfileData(w);
                                self.GetPendingApprovals(w);
                            });
                        },
                        error: function (err) {
                            BootstrapAlert.alert({
                                title: "Retrieve Failure!",
                                message: "A problem has been occurred retrieving Pinned Rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                            });
                        }
                    });
                    $.ajax({
                        method: 'get',
                        url: '/api/guardian/summary',
                        contentType: "application/json; charset=utf-8",
                        headers: {
                            'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                        },
                        success: function (v) {
                            self.dataModel.guardian.id(v.id);
                            self.dataModel.guardian.name(v.name);
                            self.dataModel.guardian.username(v.username);
                            self.dataModel.guardian.HasAvatar(v.hasAvatar);
                            self.dataModel.guardian.subscriptionDesc(v.subscriptionDescription);
                            self.dataModel.guardian.subscriptionExp(WarriorsGuild.DateConversion.convertStringToNullableDate(v.subscriptionExpires));
                            $.ajax({
                                method: 'get',
                                url: guardianDashboardViewModel.ringsByUserUrl + self.dataModel.guardian.id() + '/completed',
                                contentType: "application/json; charset=utf-8",
                                headers: {
                                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                                },
                                success: function (data) {
                                    var silver = 'Silver', gold = 'Gold', platinum = 'Platinum';
                                    $.each(data.sort(function (a, b) { return a.name.localeCompare(b.name); }), function (i, v) {
                                        var oRing = ko.mapping.fromJS(v, self.koMapperConfiguration, new WarriorsGuild.ObservableRing());
                                        switch (v.type) {
                                            case platinum:
                                                self.dataModel.guardian.CompletedPlatinumRings.push(oRing);
                                                break;
                                        }
                                    });
                                },
                                error: function (err) {
                                    BootstrapAlert.alert({
                                        title: "Retrieve Failure!",
                                        message: "A problem has been occurred retrieving completed rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                                    });
                                }
                            });
                        },
                        error: function (err) {
                            BootstrapAlert.alert({
                                title: "Retrieve Failure!",
                                message: "A problem has been occurred retrieving Pinned Rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                            });
                        }
                    });
                });
                this.get('/Dashboard', function () { this.app.runRoute('get', '#home'); });
            });
        }
        return GuardianDashboardViewModel;
    }());
    WarriorsGuild.GuardianDashboardViewModel = GuardianDashboardViewModel;
    var ObservableWarrior = /** @class */ (function () {
        function ObservableWarrior() {
            this.id = '';
            this.name = '';
            this.username = '';
            this.avatarSrc = '';
            this.HasAvatar = false;
            this.CompletedRank = ko.observable(new WarriorsGuild.ObservableRank());
            this.WorkingRank = ko.observable(new WarriorsGuild.ObservableRank());
            this.CompletedCompletionPercentage = ko.observable(0);
            this.WorkingCompletionPercentage = ko.observable(0);
            this.PinnedRings = ko.observableArray([]);
            this.PinnedCrosses = ko.observableArray([]);
            this.CompletedSilverRings = ko.observableArray([]);
            this.CompletedGoldRings = ko.observableArray([]);
            this.CompletedPlatinumRings = ko.observableArray([]);
            this.CompletedCrosses = ko.observableArray([]);
            this.RetrievingRank = ko.observable(false);
            this.RetrievingPinnedRings = ko.observable(false);
            this.RetrievingPinnedRingsError = ko.observable(false);
            this.RetrievingPendingApproval = ko.observable(false);
            this.PendingRankApproval = ko.observable();
            this.PendingRingApprovals = ko.observableArray();
            this.PendingCrossApprovals = ko.observableArray();
        }
        return ObservableWarrior;
    }());
    var ObservableGuardian = /** @class */ (function () {
        function ObservableGuardian() {
            this.id = ko.observable('');
            this.name = ko.observable('');
            this.username = ko.observable('');
            this.HasAvatar = ko.observable(false);
            this.CompletedPlatinumRings = ko.observableArray([]);
            this.subscriptionExp = ko.observable(null);
            this.subscriptionDesc = ko.observable('');
        }
        return ObservableGuardian;
    }());
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Dashboard",
    bindingMemberName: "dashboard",
    factory: WarriorsGuild.GuardianDashboardViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=guardianDashboard.viewmodel.js.map