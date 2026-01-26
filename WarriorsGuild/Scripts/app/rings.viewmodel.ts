declare var ringUrls: {
	ringsUrl: string;
	ringStatusUrl: string;
	publicRingUrl: string;
	detailUrl: string;
	recordCompletion: string;
	imageUploadBaseUrl: string;
	imageBaseUrl: string;
	uploadGuideUrl: string;
	downloadGuideUrl: string;
	proofOfCompletionUrl: string;
};
declare var ringTypes: { value: string, description: string }[];
declare var canViewAll: boolean;
declare var previewMode: boolean;

namespace WarriorsGuild {
	export class RingViewModel {
		app: AppViewModel;
		ReturnToListView: () => void;
		DetailView: KnockoutObservable<boolean>;
		ringDetail: ObservableRing;
		PreviewMode: KnockoutObservable<boolean>;
		dataModel: {
			RingsUrl: string;
			ringDetailUrl: string;
			imageUploadUrl: string;
			imageBaseUrl: string;
			Rings: KnockoutObservableArray<ObservableRing>;
			Name: KnockoutObservable<string>;
			Description: KnockoutObservable<string>;
			Type: KnockoutObservable<number>;
			selectedWarrior: KnockoutObservable<Warrior>;
			RingTypes: { value: string, description: string }[];
			reasonForReturn: KnockoutObservable<string>;
		};

		self: RingViewModel;
		constructor(app: WarriorsGuild.AppViewModel) {
			this.app = app;
			var self = this;
			this.dataModel = {
				RingsUrl: ringUrls.ringsUrl,
				ringDetailUrl: ringUrls.detailUrl,
				Rings: ko.observableArray<ObservableRing>(),
				Name: ko.observable(''),
				Description: ko.observable(''),
				Type: ko.observable(0),
				imageUploadUrl: ringUrls.imageUploadBaseUrl,
				imageBaseUrl: ringUrls.imageBaseUrl,
				selectedWarrior: app.dataModel.selectedWarrior,
				RingTypes: ringTypes,
				reasonForReturn: ko.observable<string>('')
			};
			this.DetailView = ko.observable(false);
			this.ringDetail = new ObservableRing();
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
							success: function (data: Ring[]) {
								self.dataModel.Rings.removeAll();
								$.each(data, function (i, d) {
									var oRing = <ObservableRing><any>ko.mapping.fromJS(d, self.koRingMapperConfiguration, <KnockoutObservableType<Ring>><any>new ObservableRing());
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
							success: function (data: Ring) {
								self.ringDetail.requirements.removeAll();
								ko.mapping.fromJS(data, self.koRingMapperConfiguration, <KnockoutObservableType<Ring>><any>self.ringDetail);
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
				this.post('', function () { return true; });        //for image upload
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
							success: function (data: Ring) {
								self.ringDetail.requirements.removeAll();
								ko.mapping.fromJS(data, self.koRingMapperConfiguration, <KnockoutObservableType<Ring>><any>self.ringDetail);
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
						$.each(self.dataModel.Rings(), function (i, e: ObservableRing) {
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

		private koRingMapperConfiguration = (() => {
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
						return ko.mapping.fromJS(options.data, {}, new ObservableRingRequirement());
					}
				}
			};
		})();
		HideAlerts = (): void => {
		};

		retrieveRequirements = (ringId: string): void => {
			var self = this;
			$.ajax({
				method: 'get',
				url: self.dataModel.RingsUrl + '/' + ringId + '/requirements',
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: RingRequirement[]) {
					self.ringDetail.requirements.removeAll();
					ko.mapping.fromJS(data, <KnockoutMappingOptions<RingRequirement[]>>{
						create: function (options: KnockoutMappingCreateOptions) {
							return self.CreateObservableRequirement(ringId, options.data);
						}
					}, <KnockoutObservableArray<KnockoutObservableType<RingRequirement>>><any>self.ringDetail.requirements);
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

		retrieveRingApprovalRecord = (ringId: string): void => {
			var self = this;
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
				success: function (data: PendingRingApproval) {
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
		}

		retrievePinnedRings = (): void => {
			var self = this;
			$.ajax({
				method: 'get',
				url: self.dataModel.RingsUrl + '/pinned',
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: PinnedRing[]) {
					$.each(data, (i, val) => {
						$.each(self.dataModel.Rings(), (j, ring: ObservableRing) => {
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
		}

		retrieveCompletedRings = (): void => {
			var self = this;
			$.ajax({
				method: 'get',
				url: self.dataModel.RingsUrl + '/completed',
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: Ring[]) {
					$.each(data, (i, val: Ring) => {
						$.each(self.dataModel.Rings(), (j, ring: ObservableRing) => {
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
		}

		saveNewRingOrder = (data): void => {
			var self = this;
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
				success: function (data: Ring) {
					BootstrapAlert.success({
						title: "Save Success!",
						message: "Ring Order has been updated"
					});
				},
				error: function (err: JQueryXHR) {
					data.cancelDrop = true;
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "Ring order could not be updated" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		};

		saveRing = (): void => {
			var self = this;
			this.HideAlerts();
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			$.ajax({
				method: 'post',
				url: self.dataModel.RingsUrl,
				data: ko.toJSON({ Name: this.dataModel.Name(), Description: this.dataModel.Description(), Type: this.dataModel.Type() }),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: Ring) {
					var oRing = <ObservableRing><any>ko.mapping.fromJS(data, {}, <KnockoutObservableType<Ring>><any>new ObservableRing());
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
		}
		updateRing = (data: ObservableRing, saveUrl: string): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			var requirementsToSave = [];
			$.each(data.requirements(), function (index, value) {
				var objToSave = { RingId: data.id(), ActionToComplete: value.actionToComplete(), Weight: value.weight(), Index: index };
				if (value.id !== '') objToSave['Id'] = value.id;
				requirementsToSave.push(objToSave);
			});
			this.HideAlerts();
			$.ajax({
				method: 'put',
				url: saveUrl + '/' + data.id(),
				data: ko.toJSON({ Id: data.id(), Name: data.name(), Description: data.description(), Type: data.type(), ImageUploaded: data.imageUploaded(), Requirements: requirementsToSave }),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: Ring) {
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
		}

		updateRequirements = (data: ObservableRing, saveUrl: string): void => {
			var self = this;
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
				if (value.id !== '') objToSave['Id'] = value.id;
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
			this.HideAlerts();
			if (cancelUpdate) return;
			$.ajax({
				method: 'put',
				url: saveUrl + '/' + data.id() + '/requirements',
				data: ko.toJSON(requirementsToSave),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: Ring) {
					BootstrapAlert.success({
						title: "Save Success!",
						message: "The Ring has been saved"
					});
					//self.actionSuccessMessage( 'The Ring has been saved' );
				},
				error: function (err) {
					var responseMsg = "";
					if (!err.responseText ? '' : `. ${err.responseText}` > '') {
						responseMsg += !err.responseText ? '' : `. ${err.responseText}`;
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
		}

		markAsComplete = (data: ObservableRingRequirement): void => {
			var self = this;
			this.HideAlerts();
			if (!data.requireAttachment()) {
				$.ajax({
					method: 'post',
					url: ringUrls.recordCompletion,
					data: ko.toJSON({ RingRequirementId: data.id, RingId: data.ringId }),
					contentType: "application/json; charset=utf-8",
					headers: {
						'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
					},
					success: function (ring: Ring) {
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
		}

		pin = (data: ObservableRing): void => {
			var self = this;
			this.HideAlerts();
			$.ajax({
				method: 'post',
				url: ringUrls.ringsUrl + '/pin/' + data.id(),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (ring: Ring) {
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
		}

		unpin = (data: ObservableRing): void => {
			var self = this;
			this.HideAlerts();
			$.ajax({
				method: 'post',
				url: ringUrls.ringsUrl + '/unpin/' + data.id(),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (ring: Ring) {
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
		}

		revertCompletion = (data: ObservableRingRequirement, saveUrl: string): void => {
			var self = this;
			$.ajax({
				method: 'delete',
				url: ringUrls.ringStatusUrl,
				data: ko.toJSON({ RingRequirementId: data.id, RingId: data.ringId }),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (ring: Ring) {
					BootstrapAlert.success({
						title: "Action Success!",
						message: "The requirement has been marked as incomplete"
					});
					data.attachments.splice(0);
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

		beginRequestForApproval = (): void => {
			$('#requestPromotionPopup').modal('show');
        }

		submitForApproval = (): void => {
			var self = this;
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
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not submit the ring for approval" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		ringToReturn: ObservableRing = null;
		beginReturn = (data: ObservableRing): void => {
			$('#returnReasonDialog').modal('show');
			this.ringToReturn = data;
		}

		cancelReturn = () => {
			this.dataModel.reasonForReturn('');
		}

		returnToWarrior = (data: ObservableRing): void => {
			var self = this;
			if (!data.warriorCompleted())
				return;
			$.ajax({
				method: 'post',
				url: `${ringUrls.ringStatusUrl}/${data.approvalRecordId()}/return?reason=test`,
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
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "The ring could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		confirmRingCompletion = (): void => {
			var self = this;
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
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not confirm the ring completion" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		downloadProofOfCompletionFile = (data: any): void => {
			var self = this;
			$.ajax({
				method: 'get',
				url: ringUrls.proofOfCompletionUrl + '/OneUseFileKey/' + data.id(),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (oneTimeAccessToken: string) {
					window.open(`${ringUrls.proofOfCompletionUrl}/${oneTimeAccessToken}`);
				},
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		CreateObservableRequirement = (ringId: string, data: RingRequirement): ObservableRingRequirement => {
			var self = this;
			var result = new ObservableRingRequirement();
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

			$.each(data.attachments, (i, r) => {
				result.attachments.push(ko.mapping.fromJS(r));
			});
			return result;
		}

		closePopup = (popupId: string, warriorId: string) => {
			const popupElement = document.getElementById(popupId);
			popupElement.classList.add("hidden");
			document.getElementsByClassName("ui-widget-overlay")[0].classList.add("hidden");
		};
	}

	class RingsDataModel {
		RingsUrl: string;
		ringDetailUrl: string;
		Rings: KnockoutObservableArray<Ring>;
	}
}

WarriorsGuild.app.addViewModel({
	name: "Rings",
	bindingMemberName: "rings",
	factory: WarriorsGuild.RingViewModel,
	allowUnauthorized: true
});