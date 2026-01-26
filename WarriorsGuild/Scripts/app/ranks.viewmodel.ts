declare var rankUrls: {
	ranksUrl: string;
	rankStatusUrl: string;
	publicRankUrl: string;
	detailUrl: string;
	recordCompletion: string;
	imageUploadBaseUrl: string;
	imageBaseUrl: string;
	ringStatusUrl: string;
	crossStatusUrl: string;
	crossUrl: string;
	uploadGuideUrl: string;
	downloadGuideUrl: string;
	proofOfCompletionUrl: string;
};
declare var isLoggedIn: boolean;
declare var canViewAll: boolean;
declare var previewMode: boolean;
declare var ringTypes: { value: string, description: string }[];

namespace WarriorsGuild {
	export class RankViewModel {
		app: AppViewModel;
		ReturnToListView: () => void;
		DetailView: KnockoutObservable<boolean>;
		rankDetail: ObservableRank;
		PreviewMode: KnockoutObservable<boolean>;
		ranksUrl: string = '/api/ranks';

		dataModel: {
			Ranks: KnockoutObservableArray<ObservableRank>;
			Name: KnockoutObservable<string>;
			Description: KnockoutObservable<string>;
			selectedWarrior: KnockoutObservable<Warrior>;
			ringTypes: { value: string, description: string }[];
			silverRingOptions: KnockoutObservableArray<MinimumRingDetail>;
			goldRingOptions: KnockoutObservableArray<MinimumRingDetail>;
			platinumRingOptions: KnockoutObservableArray<MinimumRingDetail>;
			crossOptions: KnockoutObservableArray<MinimumCrossDetail>;
			reasonForReturn: KnockoutObservable<string>;
			crossesToComplete: KnockoutObservableArray<MinimumCrossDetail>;
			PendingCrossApprovals: KnockoutObservableArray<PendingCrossApproval>;
			CrossDays: KnockoutObservableArray<KnockoutObservableArray<ObservableCrossDay>>;
		};
        rankService: RankService;
		constructor(app: WarriorsGuild.AppViewModel) {
			this.app = app;
			var self = this;
			this.rankService = new WarriorsGuild.RankService();
			this.dataModel = {
				Ranks: ko.observableArray<ObservableRank>(),
				Name: ko.observable(''),
				Description: ko.observable(''),
				selectedWarrior: app.dataModel.selectedWarrior,
				ringTypes: ringTypes,
				silverRingOptions: ko.observableArray<MinimumRingDetail>(),
				goldRingOptions: ko.observableArray<MinimumRingDetail>(),
				platinumRingOptions: ko.observableArray<MinimumRingDetail>(),
				crossOptions: ko.observableArray<MinimumCrossDetail>(),
				reasonForReturn: ko.observable<string>(''),
				crossesToComplete: ko.observableArray<MinimumCrossDetail>(),
				PendingCrossApprovals: ko.observableArray<PendingCrossApproval>([]),
				CrossDays: ko.observableArray<KnockoutObservableArray<ObservableCrossDay>>([])
			};
			this.DetailView = ko.observable(false);
			this.rankDetail = new ObservableRank();
			this.PreviewMode = ko.observable(previewMode);
			this.ReturnToListView = function () {
				self.DetailView(false);
			};
			app.prepareAjax(false);

			Sammy(function () {
				this.get('#ranks', function () {
					// Make a call to the protected Web API by passing in a Bearer Authorization Header
					if (canViewAll) {
						self.rankService.getRanks((data: Rank[]) => {
								self.dataModel.Ranks.removeAll();
								$.each(data, function (i, d) {
									var oRank = <ObservableRank><any>ko.mapping.fromJS(d, self.koRankMapperConfiguration, <KnockoutObservableType<Rank>><any>new ObservableRank());
									self.dataModel.Ranks.push(oRank);
								});
							},
							(err: JQueryXHR) => {
								BootstrapAlert.alert({
									title: "Retrieve Failed!",
									message: "Could not get Rank list" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
								});
							});
						self.DetailView(false);
					}
					else {
						self.rankService.getPublicRank((data: Rank) => {
							self.rankDetail.requirements.removeAll();
							ko.mapping.fromJS(data, self.koRankMapperConfiguration, <KnockoutObservableType<Rank>><any>self.rankDetail);
							if (data != null) {
								self.DetailView(true);
								self.retrieveRequirements(data.id);
							}
						},
						(err: JQueryXHR) => {
							BootstrapAlert.alert({
								title: "Retrieve Failed!",
								message: "Could not get Rank detail" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
							});
						});
					}
				});
				this.post('', function () { return true; });        //for image upload
				this.get('/ranks', function () { this.app.runRoute('get', '#ranks'); });
				this.get('/Ranks', function () { this.app.runRoute('get', '#ranks'); });
				this.get('/Ranks#detail', function () { this.app.runRoute('get', '/ranks#detail', this.params); });
				this.get('/ranks#detail', function () {
					var currContext = this as Sammy.EventContext;
					if (self.dataModel.Ranks().length === 0) {
						self.rankService.getRankDetail(currContext.params.id,
							(data: Rank) => {
								self.rankDetail.requirements.removeAll();
								ko.mapping.fromJS(data, self.koRankMapperConfiguration, <KnockoutObservableType<Rank>><any>self.rankDetail);
								self.retrieveRequirements(currContext.params.id);
								self.retrieveApprovalStatus(currContext.params.id);
							},
							(err: JQueryXHR) => {
								BootstrapAlert.alert({
									title: "Retrieve Failed!",
									message: "Could not get Rank detail" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
								});
							});
					}
					else {
						self.rankDetail.requirements.removeAll();
						$.each(self.dataModel.Ranks(), function (i, e: ObservableRank) {
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

		private koRankMapperConfiguration = (() => {
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

		retrieveRequirements = (rankId: string): void => {
			var self = this;
			this.rankService.retrieveRankRequirements(rankId,
				(data: RankRequirement[]) => {
					self.rankDetail.requirements.removeAll();
					ko.mapping.fromJS(data, <KnockoutMappingOptions<RankRequirement[]>>{
						create: function (options: KnockoutMappingCreateOptions) {
							const req = (options.data as RankRequirement);
							if (req.requireCross) {
								for (const c of req.crossesToComplete) {
									self.dataModel.crossesToComplete.push(c);
                                }
                            }
							return self.CreateObservableRequirement(rankId, options.data);
						}
					}, <KnockoutObservableArray<KnockoutObservableType<RankRequirement>>><any>self.rankDetail.requirements);
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
				},
				(err) => {
					BootstrapAlert.alert({
						title: "Requirement Retrieval Failed!",
						message: "Could not retrieve requirements" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
					//self.actionFailureMessage( 'The Ring could not be saved' );
				});
		}

		retrieveUnassignedRings = (): void => {
			var self = this;
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
						message: "Could not retrieve unassigned rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
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
				url: rankUrls.crossUrl + '/unassigned',
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: MinimumCrossDetail[]) {
					$.each(data, (i, val) => {
						let g = new MinimumCrossDetail();
						g = { ...g, ...val };
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
		}

		nullableDate(dateString: string): Date {
			return dateString === null ? null : new Date(dateString);
        }

		retrieveApprovalStatus = (rankId: string): void => {
			var self = this;
			this.rankService.getPendingApprovals(rankId,
				(data: PendingRankApproval[]) => {
					data = data.sort((a, b) => b.warriorCompletedTs.localeCompare(a.warriorCompletedTs));
					let pendingApproval = data.filter(a => a.returnedTs == null && a.guardianApprovedTs == null);
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
					let rejectedApproval = data[0];
					if (rejectedApproval?.returnedTs) {
						self.rankDetail.returnReason(rejectedApproval.returnReason);
					}
				},
				(err: JQueryXHR) => {
					BootstrapAlert.alert({
						title: "Rank Approval Retrieval Failed!",
						message: "Could not retrieve rank approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
					//self.actionFailureMessage( 'The Ring could not be saved' );
				});
		}

		saveNewRankOrder = (data): void => {
			var self = this;
			var rankOrder: { Id: string, Index: number }[] = [];
			$.each(self.dataModel.Ranks(), function (index, value) {
				var objToSave = { Id: value.id(), Index: index + 1 };
				rankOrder.push(objToSave);
			});
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			this.rankService.saveNewRankOrder(rankOrder,
				() => {
					BootstrapAlert.success({
						title: "Save Success!",
						message: "Rank Order has been updated"
					});
				},
				(err: JQueryXHR) => {
					data.cancelDrop = true;
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "Rank order could not be updated" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				});
		};

		saveRank = (): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			this.rankService.create({ Name: this.dataModel.Name(), Description: this.dataModel.Description() },
				(data: Rank) => {
					var oRank = <ObservableRank><any>ko.mapping.fromJS(data, {}, <KnockoutObservableType<Rank>><any>new ObservableRank());
					self.dataModel.Ranks.push(oRank);
					BootstrapAlert.success({
						title: "Save Success!",
						message: "The Rank has been saved"
					});
				},
				(err: JQueryXHR) => {
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "The Rank save process failed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				});
		}

		updateRank = (data: ObservableRank, saveUrl: string): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			var requirementsToSave: {
				RankId: string,
				ActionToComplete: string,
				Weight: number,
				ImageUploaded: Date,
				Index: number,
				CrossesToComplete: { id: string }[]
			}[] = [];
			$.each(data.requirements(), function (index, value) {
				var objToSave = {
					RankId: data.id(),
					ActionToComplete: value.actionToComplete(),
					Weight: value.weight(),
					ImageUploaded: data.imageUploaded(),
					Index: index,
					CrossesToComplete: value.crossesToComplete().map(c => ({ id: c.crossId() }))
				};
				if (value.id !== '') objToSave['Id'] = value.id;
				requirementsToSave.push(objToSave);
			});
			this.rankService.update({ Id: data.id(), Name: data.name(), Description: data.description(), Requirements: requirementsToSave },
				() => {
					BootstrapAlert.success({
						title: "Save Success!",
						message: "The Rank has been saved"
					});
				},
				(err: JQueryXHR) => {
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "The Rank could not be saved" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				});
		}

		updateRequirements = (): void => {
			var self = this;
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
					CrossesToComplete: value.crossesToComplete().map(c => ({ id: c.crossId })),
					InitiatedByGuardian: value.initiatedByGuardian,
					ShowAtPercent: value.showAtPercent
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
			if (cancelUpdate) return;
			$.ajax({
				method: 'put',
				url: this.ranksUrl + '/' + self.rankDetail.id() + '/requirements',
				data: ko.toJSON(requirementsToSave),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: Rank) {
					BootstrapAlert.success({
						title: "Save Success!",
						message: "The Rank has been saved"
					});
				},
				error: function (err: JQueryXHR) {
					var responseMsg = ``;
					if (!err.responseText ? '' : `. ${err.responseText}` > '') {
						responseMsg += !err.responseText ? '' : `. ${err.responseText}`;
					}
					if (err.responseJSON) {
						if (err.responseJSON.message) {
							responseMsg += err.responseJSON.message;
						}
						for (const key in err.responseJSON) {
							responseMsg += `${key}: ${err.responseJSON[key]}`;
						}
					}
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "The Rank could not be saved" + (responseMsg > `` ? '. ' + responseMsg : '')
					});
				}
			});
		}

		markAsComplete = (data: ObservableRankRequirement, event: JQueryEventObject): void => {
			var self = this;
			if (!data.requireAttachment()) {
				var validationMessages = self.validateMarkAsCompleteRequest(data);
				if (validationMessages.length === 0) {
					//var selectedRings = data.selectedRings().map(ring => ring.ringId);
					//var selectedCrosses = data.selectedCrosses().map(cross => cross.crossId);
					$.ajax({
						method: 'post',
						url: rankUrls.recordCompletion,
						data: ko.toJSON({ RankRequirementId: data.id, RankId: data.rankId }), //, Rings: selectedRings, Crosses: selectedCrosses }),
						contentType: "application/json; charset=utf-8",
						headers: {
							'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
						},
						success: function (rank: Rank) {
							BootstrapAlert.success({
								title: "Action Success!",
								message: "The requirement has been marked as complete"
							});
							data.savedCrosses(data.selectedCrosses().map(c => ko.utils.arrayFirst(self.dataModel.crossOptions(), co => co.crossId == c.crossId)));

							data.savedRings(data.selectedRings().map(val => {
								switch (data.requiredRingType()) {
									case "Silver":
										return ko.utils.arrayFirst(self.dataModel.silverRingOptions(), ro => ro.ringId == val.ringId);
									case "Gold":
										return ko.utils.arrayFirst(self.dataModel.goldRingOptions(), ro => ro.ringId == val.ringId);
									case "Platinum":
										return ko.utils.arrayFirst(self.dataModel.platinumRingOptions(), ro => ro.ringId == val.ringId);
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
						error: function (err: JQueryXHR) {
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
		}

		validateMarkAsCompleteRequest = (data: ObservableRankRequirement): string[] => {
			var messages = new Array<string>(0);
			if (data.requireRing()) {
				if (data.selectedRings().length === data.requiredRingCount()) {
					var selectedCrosses = new Array(0);
					$.each(data.selectedRings(), (i, val) => {
						if (val.ringId === null || val.ringId === undefined || val.ringId.length !== 36) {
							messages.push('One or more ring(s) does not have a valid selection');
							return false;
						}
						else {
							if ($.grep(selectedCrosses, (el, i): boolean => {
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

		revertCompletion = (data: ObservableRankRequirement, saveUrl: string): void => {
			var self = this;
			$.ajax({
				method: 'delete',
				url: rankUrls.rankStatusUrl,
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
						message: "Could not mark the given requirement as incomplete" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		completionPercentUpdatedHandler = (data: ObservableRankRequirement, percentComplete: number): void => {
			var requirementCompletion = data.weight() * percentComplete / 100;
			data.totalPercentComplete(requirementCompletion);
		}

		beginRequestForPromotion = (): void => {
			$('#requestPromotionPopup').modal('show');
		}

		submitForPromotion = (data: ObservableRank): void => {
			var self = this;
			$('#requestPromotionPopup').modal('hide');
			$.ajax({
				method: 'post',
				url: rankUrls.rankStatusUrl + '/SubmitForApproval/' + data.id(),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (rank: Rank) {
					BootstrapAlert.success({
						title: "Action Success!",
						message: "Rank submitted for promotion"
					});
					self.retrieveApprovalStatus(data.id());
				},
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not submit this Rank for promotion" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		recallRequestForPromotion = (data: ObservableRank): void => {
			var self = this;
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
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "The rank could not be recalled" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		rankToReturn: ObservableRank = null;
		beginReturn = (data: ObservableRank): void => {
			$('#returnReasonDialog').modal('show');
			this.rankToReturn = data;
		}

		cancelReturn = () => {
			this.dataModel.reasonForReturn('');
		}

		returnToWarrior = (): void => {
			let data = this.rankToReturn;
			var self = this;
			if (!data.warriorCompleted())
				return;
			this.rankService.returnToWarrior(data.approvalRecordId(),self.dataModel.reasonForReturn(),
				() => {
					self.retrieveApprovalStatus(data.id());
					BootstrapAlert.success({
						title: "Save Success!",
						message: "This rank has been returned"
					});
				},
				(err: JQueryXHR) => {
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "The rank could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				});
		}

		confirmRankCompletion = (): void => {
			var self = this;
			this.rankService.confirmRankCompletion(self.rankDetail.approvalRecordId(),
				() => {
					self.retrieveApprovalStatus(self.rankDetail.id());
					BootstrapAlert.success({
						title: "Action Success!",
						message: "The Warriors rank progress has been confirmed"
					});
				},
				(err: JQueryXHR) => {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not confirm the rank completion" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				});
		}

		downloadProofOfCompletionFile = (data: any): void => {
			var self = this;
			$.ajax({
				method: 'get',
				url: rankUrls.proofOfCompletionUrl + '/OneUseFileKey/' + data.id(),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (oneTimeAccessToken: string) {
					window.open(`${rankUrls.proofOfCompletionUrl}/${oneTimeAccessToken}`);
				},
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		CreateObservableRequirement = (rankId: string, data: RankRequirement): ObservableRankRequirement => {
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
			result.requireAttachment(data.requireAttachment && isLoggedIn);
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

			data.savedRings.forEach( (r) => {
				result.savedRings.push(r);
			});

			if (data.crossesToComplete.length > 0) {
				result.crossesToComplete.removeAll();
				var totalComplete = 0;
				$.each(data.crossesToComplete, (i, r) => {
					totalComplete += r.percentCompleted;
					result.crossesToComplete.push(r);
				});
				var requirementCompletion = data.weight * totalComplete / 100;
				result.totalPercentComplete(requirementCompletion);
			}

			$.each(data.savedCrosses, (i, r) => {
				result.savedCrosses.push(r);
			});

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

	class RanksDataModel {
		RanksUrl: string;
		rankDetailUrl: string;
		Ranks: KnockoutObservableArray<Rank>;
	}
}

WarriorsGuild.app.addViewModel({
	name: "Ranks",
	bindingMemberName: "ranks",
	factory: WarriorsGuild.RankViewModel,
	allowUnauthorized: true
});