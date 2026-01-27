var WarriorsGuild;
(function (WarriorsGuild) {
    var RingViewModel = /** @class */ (function () {
        function RingViewModel(app) {
            var _this = this;
            this.koRingMapperConfiguration = (function () {
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
                            return ko.mapping.fromJS(options.data, {}, new WarriorsGuild.ObservableRingRequirement());
                        }
                    }
                };
            })();
            this.HideAlerts = function () {
            };
            this.retrieveRequirements = function (ringId) {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: self.dataModel.RingsUrl + '/' + ringId + '/requirements',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        self.ringDetail.requirements.removeAll();
                        ko.mapping.fromJS(data, {
                            create: function (options) {
                                return self.CreateObservableRequirement(ringId, options.data);
                            }
                        }, self.ringDetail.requirements);
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
            this.retrieveRingApprovalRecord = function (ringId) {
                var self = _this;
                self.ringDetail.approvalRecordId(null);
                self.ringDetail.warriorCompletedTs(null);
                self.ringDetail.guardianReviewedTs(null);
                $.ajax({
                    method: 'get',
                    url: ringUrls.ringStatusUrl + '/' + ringId + '/PendingApproval',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        if (data != null) {
                            self.ringDetail.warriorCompletedTs(data.warriorCompleted === null ? null : data.warriorCompleted);
                            self.ringDetail.guardianReviewedTs(data.guardianConfirmed === null ? null : data.guardianConfirmed);
                            self.ringDetail.approvalRecordId(data.approvalRecordId);
                        }
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Requirement Retrieval Failed!",
                            message: "Could not retrieve ring status"
                        });
                        //self.actionFailureMessage( 'The Ring could not be saved' );
                    }
                });
            };
            this.retrievePinnedRings = function () {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: self.dataModel.RingsUrl + '/pinned',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data, function (i, val) {
                            $.each(self.dataModel.Rings(), function (j, ring) {
                                if (ring.id() === val.ringId) {
                                    ring.isPinned(true);
                                    return false;
                                }
                            });
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Pins Retrieval Failed!",
                            message: "Could not retrieve pins"
                        });
                        //self.actionFailureMessage( 'The Ring could not be saved' );
                    }
                });
            };
            this.retrieveCompletedRings = function () {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: self.dataModel.RingsUrl + '/completed',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        $.each(data, function (i, val) {
                            $.each(self.dataModel.Rings(), function (j, ring) {
                                if (ring.id() === val.id) {
                                    ring.isCompleted(true);
                                    return false;
                                }
                            });
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Completed Ring Retrieval Failed!",
                            message: "Could not retrieve completed rings"
                        });
                        //self.actionFailureMessage( 'The Ring could not be saved' );
                    }
                });
            };
            this.saveNewRingOrder = function (data) {
                var self = _this;
                var ringOrder = [];
                $.each(self.dataModel.Rings(), function (index, value) {
                    var objToSave = { Id: value.id(), Index: index + 1 };
                    ringOrder.push(objToSave);
                });
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                $.ajax({
                    method: 'post',
                    async: false,
                    timeout: 5000,
                    url: self.dataModel.RingsUrl + "/Order",
                    data: ko.toJSON(ringOrder),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "Ring Order has been updated"
                        });
                    },
                    error: function (err) {
                        data.cancelDrop = true;
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "Ring order could not be updated" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.saveRing = function () {
                var self = _this;
                _this.HideAlerts();
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                $.ajax({
                    method: 'post',
                    url: self.dataModel.RingsUrl,
                    data: ko.toJSON({ Name: _this.dataModel.Name(), Description: _this.dataModel.Description(), Type: _this.dataModel.Type() }),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        var oRing = ko.mapping.fromJS(data, {}, new WarriorsGuild.ObservableRing());
                        oRing.pin = self.pin;
                        oRing.unpin = self.unpin;
                        self.dataModel.Rings.push(oRing);
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "The Ring has been saved"
                        });
                        //self.actionSuccessMessage( 'The Ring has been saved' );
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The Ring could not be saved"
                        });
                        //self.actionFailureMessage( 'The Ring could not be saved' );
                    }
                });
            };
            this.updateRing = function (data, saveUrl) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var requirementsToSave = [];
                $.each(data.requirements(), function (index, value) {
                    var objToSave = { RingId: data.id(), ActionToComplete: value.actionToComplete(), Weight: value.weight(), Index: index };
                    if (value.id !== '')
                        objToSave['Id'] = value.id;
                    requirementsToSave.push(objToSave);
                });
                _this.HideAlerts();
                $.ajax({
                    method: 'put',
                    url: saveUrl + '/' + data.id(),
                    data: ko.toJSON({ Id: data.id(), Name: data.name(), Description: data.description(), Type: data.type(), ImageUploaded: data.imageUploaded(), Requirements: requirementsToSave }),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "The Ring has been saved"
                        });
                        //self.actionSuccessMessage( 'The Ring has been saved' );
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The Ring could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                        //self.actionFailureMessage( 'The Ring could not be saved' );
                    }
                });
            };
            this.updateRequirements = function (data, saveUrl) {
                var self = _this;
                // Make a call to the protected Web API by passing in a Bearer Authorization Header
                var requirementsToSave = [];
                var cancelUpdate = false;
                $.each(data.requirements(), function (index, value) {
                    var objToSave = {
                        RingId: data.id(),
                        ActionToComplete: value.actionToComplete(),
                        Weight: value.weight(),
                        Index: index,
                        RequireAttachment: value.requireAttachment(),
                        SeeHowLink: value.seeHowLink()
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
                _this.HideAlerts();
                if (cancelUpdate)
                    return;
                $.ajax({
                    method: 'put',
                    url: saveUrl + '/' + data.id() + '/requirements',
                    data: ko.toJSON(requirementsToSave),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "The Ring has been saved"
                        });
                        //self.actionSuccessMessage( 'The Ring has been saved' );
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
                                responseMsg += key + ": " + err.responseJSON[key];
                            }
                        }
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The Ring could not be saved" + (responseMsg > "" ? '. ' + responseMsg : '')
                        });
                    }
                });
            };
            this.markAsComplete = function (data) {
                var self = _this;
                _this.HideAlerts();
                if (!data.requireAttachment()) {
                    $.ajax({
                        method: 'post',
                        url: ringUrls.recordCompletion,
                        data: ko.toJSON({ RingRequirementId: data.id, RingId: data.ringId }),
                        contentType: "application/json; charset=utf-8",
                        headers: {
                            'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                        },
                        success: function (ring) {
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
                                message: "Could not mark the given requirement as complete"
                            });
                        }
                    });
                }
            };
            this.pin = function (data) {
                var self = _this;
                _this.HideAlerts();
                $.ajax({
                    method: 'post',
                    url: ringUrls.ringsUrl + '/pin/' + data.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (ring) {
                        data.isPinned(true);
                        BootstrapAlert.success({
                            title: "Pin Success!",
                            message: "The ring has been pinned"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Pin Failed!",
                            message: "Could not pin the ring"
                        });
                    }
                });
            };
            this.unpin = function (data) {
                var self = _this;
                _this.HideAlerts();
                $.ajax({
                    method: 'post',
                    url: ringUrls.ringsUrl + '/unpin/' + data.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (ring) {
                        data.isPinned(false);
                        BootstrapAlert.success({
                            title: "Unpin Success!",
                            message: "The ring has been un-pinned"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Unpin Failed!",
                            message: "Could not un-pin the ring"
                        });
                    }
                });
            };
            this.revertCompletion = function (data, saveUrl) {
                var self = _this;
                $.ajax({
                    method: 'delete',
                    url: ringUrls.ringStatusUrl,
                    data: ko.toJSON({ RingRequirementId: data.id, RingId: data.ringId }),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (ring) {
                        BootstrapAlert.success({
                            title: "Action Success!",
                            message: "The requirement has been marked as incomplete"
                        });
                        data.attachments.splice(0);
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
            this.beginRequestForApproval = function () {
                $('#requestPromotionPopup').modal('show');
            };
            this.submitForApproval = function () {
                var self = _this;
                $('#requestPromotionPopup').modal('hide');
                $.ajax({
                    method: 'post',
                    url: ringUrls.ringStatusUrl + '/' + self.ringDetail.id() + '/SubmitForApproval',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (data) {
                        self.ringDetail.warriorCompletedTs(new Date());
                        self.ringDetail.approvalRecordId(data.id);
                        BootstrapAlert.success({
                            title: "Action Success!",
                            message: "The ring has been submitted for approval"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not submit the ring for approval" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.ringToReturn = null;
            this.beginReturn = function (data) {
                $('#returnReasonDialog').modal('show');
                _this.ringToReturn = data;
            };
            this.cancelReturn = function () {
                _this.dataModel.reasonForReturn('');
            };
            this.returnToWarrior = function (data) {
                var self = _this;
                if (!data.warriorCompleted())
                    return;
                $.ajax({
                    method: 'post',
                    url: "".concat(ringUrls.ringStatusUrl, "/").concat(data.approvalRecordId(), "/return?reason=test"),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        data.warriorCompletedTs(null);
                        BootstrapAlert.success({
                            title: "Save Success!",
                            message: "This ring has been returned"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Save Failed!",
                            message: "The ring could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.confirmRingCompletion = function () {
                var self = _this;
                $.ajax({
                    method: 'post',
                    url: ringUrls.ringStatusUrl + '/' + self.ringDetail.approvalRecordId() + '/ApproveProgress',
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function () {
                        self.ringDetail.guardianReviewedTs(new Date());
                        BootstrapAlert.success({
                            title: "Action Success!",
                            message: "The Warriors ring progress has been confirmed"
                        });
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not confirm the ring completion" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.downloadProofOfCompletionFile = function (data) {
                var self = _this;
                $.ajax({
                    method: 'get',
                    url: ringUrls.proofOfCompletionUrl + '/OneUseFileKey/' + data.id(),
                    contentType: "application/json; charset=utf-8",
                    headers: {
                        'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                    },
                    success: function (oneTimeAccessToken) {
                        window.open("".concat(ringUrls.proofOfCompletionUrl, "/").concat(oneTimeAccessToken));
                    },
                    error: function (err) {
                        BootstrapAlert.alert({
                            title: "Action Failed!",
                            message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                        });
                    }
                });
            };
            this.CreateObservableRequirement = function (ringId, data) {
                var self = _this;
                var result = new WarriorsGuild.ObservableRingRequirement();
                result.id = data.id;
                result.ringId = ringId;
                result.weight(data.weight);
                result.actionToComplete(data.actionToComplete);
                result.markAsComplete = self.markAsComplete;
                result.revertCompletion = self.revertCompletion;
                result.warriorCompletedTs(data.warriorCompletedTs !== null ? new Date(data.warriorCompletedTs) : null);
                result.guardianReviewedTs(data.guardianReviewedTs !== null ? new Date(data.guardianReviewedTs) : null);
                result.requireAttachment(data.requireAttachment && isLoggedIn);
                result.seeHowLink(data.seeHowLink);
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
            this.dataModel = {
                RingsUrl: ringUrls.ringsUrl,
                ringDetailUrl: ringUrls.detailUrl,
                Rings: ko.observableArray(),
                Name: ko.observable(''),
                Description: ko.observable(''),
                Type: ko.observable(0),
                imageUploadUrl: ringUrls.imageUploadBaseUrl,
                imageBaseUrl: ringUrls.imageBaseUrl,
                selectedWarrior: app.dataModel.selectedWarrior,
                RingTypes: ringTypes,
                reasonForReturn: ko.observable('')
            };
            this.DetailView = ko.observable(false);
            this.ringDetail = new WarriorsGuild.ObservableRing();
            this.PreviewMode = ko.observable(previewMode);
            this.ReturnToListView = function () {
                self.DetailView(false);
            };
            app.prepareAjax(false);
            Sammy(function () {
                this.get('#rings', function () {
                    self.HideAlerts();
                    // Make a call to the protected Web API by passing in a Bearer Authorization Header
                    if (canViewAll) {
                        $.ajax({
                            method: 'get',
                            url: self.dataModel.RingsUrl,
                            contentType: "application/json; charset=utf-8",
                            //headers: {
                            //	'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                            //},
                            success: function (data) {
                                self.dataModel.Rings.removeAll();
                                $.each(data, function (i, d) {
                                    var oRing = ko.mapping.fromJS(d, self.koRingMapperConfiguration, new WarriorsGuild.ObservableRing());
                                    oRing.pin = self.pin;
                                    oRing.unpin = self.unpin;
                                    self.dataModel.Rings.push(oRing);
                                });
                                self.retrievePinnedRings();
                                self.retrieveCompletedRings();
                            },
                            error: function (err) {
                                BootstrapAlert.alert({
                                    title: "Ring Retrieval Failed!",
                                    message: "Could not retrieve rings"
                                });
                                //self.actionFailureMessage( 'The Ring could not be saved' );
                            }
                        });
                        self.DetailView(false);
                    }
                    else {
                        $.ajax({
                            method: 'get',
                            url: ringUrls.publicRingUrl,
                            contentType: "application/json; charset=utf-8",
                            //headers: {
                            //	'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                            //},
                            success: function (data) {
                                self.ringDetail.requirements.removeAll();
                                ko.mapping.fromJS(data, self.koRingMapperConfiguration, self.ringDetail);
                                if (data != null) {
                                    self.DetailView(true);
                                    self.retrieveRequirements(data.id);
                                }
                            },
                            error: function (err) {
                                BootstrapAlert.alert({
                                    title: "Ring Retrieval Failed!",
                                    message: "Could not retrieve ring detail"
                                });
                                //self.actionFailureMessage( 'The Ring could not be saved' );
                            }
                        });
                    }
                });
                this.post('', function () { return true; }); //for image upload
                this.get('/rings', function () { this.app.runRoute('get', '#rings'); });
                this.get('/Rings', function () { this.app.runRoute('get', '#rings'); });
                this.get('/Rings#detail', function () { this.app.runRoute('get', '/rings#detail', this.params); });
                this.get('/rings#detail', function () {
                    var currContext = this;
                    if (self.dataModel.Rings().length === 0) {
                        self.HideAlerts();
                        $.ajax({
                            method: 'get',
                            url: self.dataModel.RingsUrl + '/' + currContext.params.id,
                            contentType: "application/json; charset=utf-8",
                            headers: {
                                'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                            },
                            success: function (data) {
                                self.ringDetail.requirements.removeAll();
                                ko.mapping.fromJS(data, self.koRingMapperConfiguration, self.ringDetail);
                                self.retrieveRequirements(currContext.params.id);
                                self.retrieveRingApprovalRecord(currContext.params.id);
                            },
                            error: function (err) {
                                BootstrapAlert.alert({
                                    title: "Ring Retrieval Failed!",
                                    message: "Could not retrieve ring detail"
                                });
                                //self.actionFailureMessage( 'The Ring could not be saved' );
                            }
                        });
                    }
                    else {
                        self.ringDetail.requirements.removeAll();
                        $.each(self.dataModel.Rings(), function (i, e) {
                            if (e.id() === currContext.params.id) {
                                var ringToEdit = ko.mapping.toJS(e);
                                ko.mapping.fromJS(ringToEdit, self.koRingMapperConfiguration, self.ringDetail);
                                self.retrieveRequirements(currContext.params.id);
                                self.retrieveRingApprovalRecord(currContext.params.id);
                                ringToEdit.pin = self.pin;
                                ringToEdit.unpin = self.unpin;
                                return false;
                            }
                        });
                    }
                    self.DetailView(true);
                });
            });
        }
        return RingViewModel;
    }());
    WarriorsGuild.RingViewModel = RingViewModel;
    var RingsDataModel = /** @class */ (function () {
        function RingsDataModel() {
        }
        return RingsDataModel;
    }());
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Rings",
    bindingMemberName: "rings",
    factory: WarriorsGuild.RingViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=rings.viewmodel.js.map