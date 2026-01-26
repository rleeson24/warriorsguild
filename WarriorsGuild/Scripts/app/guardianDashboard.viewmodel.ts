declare var guardianDashboardViewModel: {
	readOnly: boolean;
	profileId: string;
	getCurrentAndWorkingRank: string;
	markAsComplete: string;
	rankImageBase: string;
	fullName: string;
	imageUploaded: Date;
	currentAndWorkingRank: WarriorsGuild.MyRankViewModel;
	ranksUrl: string;
	blogUrl: string;
	warriorListUrl: string;
	ranksByUserUrl: string;
	ringsByUserUrl: string;
	crossesByUserUrl: string;
	rankPendingApprovalUrl: string;
	ringsPendingApprovalUrl: string;
	crossesPendingApprovalUrl: string;
	rankApprovalUrl: string;
	rankStatusUrl: string;
	proofOfCompletionUrl: string;
};

namespace WarriorsGuild {
	export class GuardianDashboardViewModel {
		app: AppViewModel;
		dataModel: {
			BlogEntries: KnockoutObservableArray<object>;
			Warriors: KnockoutObservableArray<ObservableWarrior>;
			guardian: ObservableGuardian;
			PinnedRings: KnockoutObservableArray<MinimumRingDetail>;
			reasonForReturn: KnockoutObservable<string>;
		};
		rankService: RankService;
		RetrievingPinnedRings: KnockoutObservable<boolean>;
		constructor(app: WarriorsGuild.AppViewModel, dataModel) {
			var self = this;
			self.app = app;
			this.rankService = new WarriorsGuild.RankService();
			this.dataModel = {
				BlogEntries: ko.observableArray([]),
				Warriors: ko.observableArray<ObservableWarrior>(),
				guardian: new ObservableGuardian(),
				PinnedRings: ko.observableArray<MinimumRingDetail>([]),
				reasonForReturn: ko.observable<string>('')
			};
			self.dataModel.BlogEntries = ko.observableArray([]);
			app.prepareAjax();
			self.RetrievingPinnedRings = ko.observable<boolean>(false);
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
						url: guardianDashboardViewModel.warriorListUrl,
						contentType: "application/json; charset=utf-8",
						headers: {
							'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
						},
						success: function (data: Warrior[]) {
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
						error: function (err: JQueryXHR) {
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
						success: function (v: Guardian) {
							self.dataModel.guardian.id( v.id );
							self.dataModel.guardian.name(v.name );
							self.dataModel.guardian.username( v.username );
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
								success: function (data: Ring[]) {
									var silver = 'Silver', gold = 'Gold', platinum = 'Platinum';
									$.each(data.sort((a, b) => a.name.localeCompare(b.name)), function (i, v) {
										var oRing = <ObservableRing><any>ko.mapping.fromJS(v, self.koMapperConfiguration, <KnockoutObservableType<Ring>><any>new ObservableRing());
										switch (v.type) {
											case platinum:
												self.dataModel.guardian.CompletedPlatinumRings.push(oRing);
												break;
										}
									});
								},
								error: function (err: JQueryXHR) {
									BootstrapAlert.alert({
										title: "Retrieve Failure!",
										message: "A problem has been occurred retrieving completed rings" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
									});
								}
							});
						},
						error: function (err: JQueryXHR) {
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

		GetProfileData = (warrior: ObservableWarrior): void => {
			var self = this;
			warrior.RetrievingRank(true);
			//Make a call to the protected Web API by passing in a Bearer Authorization Header
			this.getWorkingAndCompletedRanks(warrior);
			$.ajax({
				method: 'get',
				url: guardianDashboardViewModel.ringsByUserUrl + warrior.id + '/pinned',
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: Ring[]) {
					$.each(data, function (i, r) {
						let m = new MinimumRingDetail();
						m.hasImage = r.imageUploaded !== null;
						m.ringId = r.id;
						m.imageExtension = r.imageExtension;
						m.name = r.name;
						m.imgSrcAttr = m.hasImage ? "/images/rings/" + r.id + r.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png"
						warrior.PinnedRings.push(m);
					});
				},
				error: function (err: JQueryXHR) {
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
				success: function (data: Cross[]) {
					$.each(data, function (i, c) {
						let m = new MinimumCrossDetail();
						m.hasImage = c.imageUploaded != null;
						m.crossId(c.id);
						m.imageExtension = c.imageExtension;
						m.name = c.name;
						m.imgSrcAttr = m.hasImage ? "/images/rings/" + c.id + c.imageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png"
						warrior.PinnedCrosses.push(m);
					});
				},
				error: function (err: JQueryXHR) {
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
				success: function (data: Ring[]) {
					var silver = 'Silver', gold = 'Gold', platinum = 'Platinum';
					$.each(data.sort((a, b) => a.name.localeCompare(b.name)), function (i, v) {
						var oRing = <ObservableRing><any>ko.mapping.fromJS(v, self.koMapperConfiguration, <KnockoutObservableType<Ring>><any>new ObservableRing());
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
				error: function (err: JQueryXHR) {
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
				success: function (data: Cross[]) {
					$.each(data.sort((a, b) => a.name.localeCompare(b.name)), function (i, v) {
						var oCross = <ObservableCross><any>ko.mapping.fromJS(v, self.koMapperConfiguration, <KnockoutObservableType<Cross>><any>new ObservableCross());
						warrior.CompletedCrosses.push(oCross);
					});
				},
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Retrieve Failure!",
						message: "A problem has been occurred retrieving completed crosses" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		};

		getWorkingAndCompletedRanks = (warrior: ObservableWarrior): void => {
			let self = this;
			$.ajax({
				method: 'get',
				url: guardianDashboardViewModel.ranksByUserUrl + warrior.id,
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: MyRankViewModel) {
					warrior.RetrievingRank(false);
					warrior.CompletedRank().requirements.removeAll();
					if (data.completedRank !== null) {
						ko.mapping.fromJS(data.completedRank, <KnockoutMappingOptions<Rank>>self.koMapperConfiguration, <KnockoutObservableType<Rank>><any>warrior.CompletedRank);
						warrior.CompletedRank().subRank(self.calculateSubRank(data.completedRank.percentComplete));
					}
					warrior.WorkingRank().requirements.removeAll();
					ko.mapping.fromJS(data.workingRank, <KnockoutMappingOptions<Rank>>self.koMapperConfiguration, <KnockoutObservableType<Rank>><any>warrior.WorkingRank);
					self.retrieveRequirements(warrior.WorkingRank());
					warrior.WorkingCompletionPercentage(data.workingCompletionPercentage);
					warrior.CompletedCompletionPercentage(data.completedCompletionPercentage);
				},
				error: function (err: JQueryXHR) {
					warrior.RetrievingRank(false);
					BootstrapAlert.alert({
						title: "Retrieve Failure!",
						message: "A problem has been occurred attempting to retrieve your rank" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		GetPendingApprovals = (warrior: ObservableWarrior): void => {
			var self = this;
			warrior.RetrievingPendingApproval(true);
			warrior.RetrievingPinnedRings(true);
			$.ajax({
				method: 'get',
				url: guardianDashboardViewModel.rankPendingApprovalUrl + warrior.id,
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (data: PendingRankApproval) {
					warrior.RetrievingRank(false);
					if (data != null) {
						data.hasImage = data.rankImageUploaded != null;
						data.pendingSubRank = self.calculateSubRank(data.percentComplete);
						data.imgSrcAttr = data.hasImage ? '/images/ranks/' + data.rankId + data.imageExtension : '/images/logo/Warriors-Guild-icon-sm-wide.png';
						$.each(data.unconfirmedRequirements, (i, r) => {
							r.savedRings = self.MapToMinimumRingDetail(r.savedRings);
							r.savedCrosses = self.MapToMinimumCrossDetail(r.savedCrosses);
							r.actionToCompleteLinked = r.actionToComplete?.replace(/\n/g, '<br />').replace(/\[link ([a-zA-Z ]+)\]/g, `<a href='${r.seeHowLink}' target='_blank'>$1</a>`);
						});
					}
					warrior.PendingRankApproval(data);
					warrior.RetrievingPendingApproval(false);
				},
				error: function (err: JQueryXHR) {
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
				success: function (data: PendingRingApproval[]) {
					//warrior.RetrievingRing( false );
					$.each(data, function (index, d) {
						d.hasImage = d.ringImageUploaded != null;
						d.imgSrcAttr = d.hasImage ? '/images/rings/' + d.ringId + d.imageExtension : '/images/logo/Warriors-Guild-icon-sm-wide.png';
					});
					warrior.PendingRingApprovals(data);
					warrior.RetrievingPinnedRings(false);
				},
				error: function (err: JQueryXHR) {
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
				success: function (data: PendingCrossApproval[]) {
					//warrior.RetrievingRing( false );
					$.each(data, function (index, d) {
						d.hasImage = d.crossImageUploaded != null;
						d.imgSrcAttr = d.hasImage ? '/images/crosses/' + d.crossId + d.imageExtension + '?' : '/images/logo/Warriors-Guild-icon-sm-wide.png';
					});
					warrior.PendingCrossApprovals(data);
				},
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Retrieve Failure!",
						message: "A problem has been occurred attempting to retrieve the pending cross approvals" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		approvalRecordId: string = null;
		warrior: ObservableWarrior = null;
		beginReturn = (approvalRecordId: string, warrior: ObservableWarrior): void => {
			$('#returnReasonDialog').modal('show');
			this.approvalRecordId = approvalRecordId;
			this.warrior = warrior;
		}

		cancelReturn = () => {
			this.dataModel.reasonForReturn('');
		}
		
		returnRank = (): void => {
			let approvalRecordId = this.approvalRecordId;
			let warrior = this.warrior;
			var self = this;
			$.ajax({
				method: 'post',
				url: `${guardianDashboardViewModel.rankStatusUrl}/${approvalRecordId}/return?reason=${self.dataModel.reasonForReturn()}`,
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
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Save Failed!",
						message: "The rank could not be returned" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
		}

		promoteWarrior = (approvalRecordId: string, warrior: ObservableWarrior): void => {
			var self = this;
			$.ajax({
				method: 'post',
				url: guardianDashboardViewModel.rankStatusUrl + '/' + approvalRecordId + '/ApproveProgress',
				data: ko.toJSON({ ApprovalRecordId: approvalRecordId, UserId: warrior.id }),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (rank: Rank) {
					BootstrapAlert.success({
						title: "Action Success!",
						message: "The Warrior has been Promoted"
					});
					self.getWorkingAndCompletedRanks(warrior);
					warrior.PendingRankApproval(null);
				},
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not Promote the Warrior" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
				}
			});
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

		setCurrentAndWorkingRanks = (warrior: ObservableWarrior, data: MyRankViewModel): void => {
			var self = this;
			warrior.CompletedRank().requirements.removeAll();
			ko.mapping.fromJS(data.completedRank, <KnockoutMappingOptions<Rank>>self.koMapperConfiguration, <KnockoutObservableType<Rank>><any>warrior.CompletedRank);
			ko.mapping.fromJS(data.workingRank, <KnockoutMappingOptions<Rank>>self.koMapperConfiguration, <KnockoutObservableType<Rank>><any>warrior.WorkingRank);
			self.retrieveRequirements(warrior.WorkingRank())
			warrior.WorkingCompletionPercentage(data.workingCompletionPercentage);
			warrior.CompletedCompletionPercentage(data.completedCompletionPercentage);
		};

		retrieveRequirements = (rank: ObservableRank): void => {
			var self = this;
			this.rankService.retrieveRankRequirements(rank.id(),
				(data: RankRequirement[]) => {
					rank.requirements.removeAll();
					ko.mapping.fromJS(data, <KnockoutMappingOptions<RankRequirement[]>>{
						create: function (options: KnockoutMappingCreateOptions) {
							return self.CreateObservableRankRequirement(rank.id(), options.data);
						}
					}, <KnockoutObservableArray<KnockoutObservableType<RankRequirement>>><any>rank.requirements);
				},
				(err: JQueryXHR) => {
					BootstrapAlert.alert({
						title: "Requirement Retrieval Failed!",
						message: "Could not retrieve requirements" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
					});
                }
			);
		}

		markAsComplete = (data: ObservableRankRequirement, saveUrl: string): void => {
			if (!guardianDashboardViewModel.readOnly) {
				var self = this;
				self.rankService.markRequirementCompleteByGuardian(data.rankId, data.id,
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
						BootstrapAlert.alert({
							title: "Action Failed!",
							message: "Could not mark the given requirement as complete" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
						});
					}
				);
			}
		}

		revertCompletion = (data: ObservableRankRequirement, saveUrl: string): void => {
			if (!guardianDashboardViewModel.readOnly) {
				var self = this;
				$.ajax({
					method: 'post',
					url: guardianDashboardViewModel.markAsComplete,
					data: ko.toJSON({ RankRequirementId: data.id, RankId: data.rankId }),
					contentType: "application/json; charset=utf-8",
					headers: {
						'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
					},
					success: function (rank: Rank) {
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
					error: function (err: JQueryXHR) {
						BootstrapAlert.alert({
							title: "Action Failed!",
							message: "Could not mark the given requirement as complete" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
						});
					}
				});
			}
		}

		downloadProofOfCompletionFile = (data: any): void => {
			var self = this;
			$.ajax({
				method: 'get',
				url: guardianDashboardViewModel.proofOfCompletionUrl + '/OneUseFileKey/' + data.id,
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				success: function (oneTimeAccessToken: string) {
					window.open(`${guardianDashboardViewModel.proofOfCompletionUrl}/${oneTimeAccessToken}`);
				},
				error: function (err: JQueryXHR) {
					BootstrapAlert.alert({
						title: "Action Failed!",
						message: "Could not download the attachment" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
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

		CreateObservableRingRequirement = (rankOrRing: Ring, data: RingRequirement): ObservableRingRequirement => {
			var self = this;
			var result = new ObservableRingRequirement();
			result.id = data.id;
			result.ringId = rankOrRing.id;
			result.weight(data.weight);
			result.actionToComplete(data.actionToComplete);
			result.markAsComplete = self.markAsComplete;
			$.each(rankOrRing.statuses, function (i, s: RingStatus) {
				if (s.ringRequirementId === data.id) {
					result.warriorCompletedTs(new Date(s.warriorCompleted));
					result.guardianReviewedTs(s.guardianCompleted !== null ? new Date(s.guardianCompleted) : null);
				}
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

		MapToMinimumRingDetail = (rings: MinimumRingDetail[]): MinimumRingDetail[] => {
			let result: MinimumRingDetail[] = [];
			$.each(rings, (i, r) => {
				let m = new MinimumRingDetail();
				m.hasImage = r.hasImage;
				m.ringId = r.ringId;
				m.imageExtension = r.imageExtension;
				m.name = r.name;
				m.imgSrcAttr = r.imgSrcAttr;
				result.push(m);
			});
			return result;
		};

		MapToMinimumCrossDetail = (rings: MinimumCrossDetail[]): MinimumCrossDetail[] => {
			let result: MinimumCrossDetail[] = [];
			$.each(rings, (i, r) => {
				let m = new MinimumCrossDetail();
				m.hasImage = r.hasImage;
				m.crossId = r.crossId;
				m.imageExtension = r.imageExtension;
				m.name = r.name;
				m.imgSrcAttr = r.imgSrcAttr;
				result.push(m);
			});
			return result;
		};

		showPopup = (popupId: string, warriorId: string) => {
			$.each($(`.popuptext:not(#${popupId}${warriorId})`), (index, el) => {
				el.classList.remove('show');
			});
			const popupElement = document.getElementById(popupId + warriorId);
			popupElement ? popupElement.classList.toggle(`show`) : null;
		};

		closePopup = (popupId: string, warriorId: string) => {
			const popupElement = document.getElementById(popupId + warriorId);
			popupElement.classList.toggle(`show`);
		};
	}

	class ObservableWarrior {
		id: string = '';
		name: string = '';
		username: string = '';
		avatarSrc: string = '';
		HasAvatar: boolean = false;
		CompletedRank: KnockoutObservable<ObservableRank> = ko.observable<ObservableRank>(new ObservableRank());
		WorkingRank: KnockoutObservable<ObservableRank> = ko.observable<ObservableRank>(new ObservableRank());
		CompletedCompletionPercentage: KnockoutObservable<number> = ko.observable<number>(0);
		WorkingCompletionPercentage: KnockoutObservable<number> = ko.observable<number>(0);
		PinnedRings: KnockoutObservableArray<MinimumRingDetail> = ko.observableArray<MinimumRingDetail>([]);
		PinnedCrosses: KnockoutObservableArray<MinimumCrossDetail> = ko.observableArray<MinimumCrossDetail>([]);
		CompletedSilverRings: KnockoutObservableArray<ObservableRing> = ko.observableArray<ObservableRing>([]);
		CompletedGoldRings: KnockoutObservableArray<ObservableRing> = ko.observableArray<ObservableRing>([]);
		CompletedPlatinumRings: KnockoutObservableArray<ObservableRing> = ko.observableArray<ObservableRing>([]);
		CompletedCrosses: KnockoutObservableArray<ObservableCross> = ko.observableArray<ObservableCross>([]);
		RetrievingRank: KnockoutObservable<boolean> = ko.observable(false);
		RetrievingPinnedRings: KnockoutObservable<boolean> = ko.observable(false);
		RetrievingPinnedRingsError: KnockoutObservable<boolean> = ko.observable(false);
		RetrievingPendingApproval: KnockoutObservable<boolean> = ko.observable(false);
		PendingRankApproval: KnockoutObservable<PendingRankApproval> = ko.observable<PendingRankApproval>();
		PendingRingApprovals: KnockoutObservableArray<PendingRingApproval> = ko.observableArray<PendingRingApproval>();
		PendingCrossApprovals: KnockoutObservableArray<PendingCrossApproval> = ko.observableArray<PendingCrossApproval>();
	}

	class ObservableGuardian {
		id: KnockoutObservable<string> = ko.observable<string>('');
		name: KnockoutObservable<string> = ko.observable<string>('');
		username: KnockoutObservable<string> = ko.observable<string>('');
		HasAvatar: KnockoutObservable<boolean> = ko.observable<boolean>(false);
		CompletedPlatinumRings: KnockoutObservableArray<ObservableRing> = ko.observableArray<ObservableRing>([]);
		subscriptionExp: KnockoutObservable<Date> = ko.observable<Date>(null);
		subscriptionDesc: KnockoutObservable<string> = ko.observable<string>('');
	}
}

WarriorsGuild.app.addViewModel({
	name: "Dashboard",
	bindingMemberName: "dashboard",
	factory: WarriorsGuild.GuardianDashboardViewModel,
	allowUnauthorized: true
});
