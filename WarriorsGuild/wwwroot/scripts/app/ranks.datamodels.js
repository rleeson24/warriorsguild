var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var WarriorsGuild;
(function (WarriorsGuild) {
    var Rank = /** @class */ (function () {
        function Rank() {
        }
        return Rank;
    }());
    WarriorsGuild.Rank = Rank;
    var RankStatus = /** @class */ (function () {
        function RankStatus() {
        }
        return RankStatus;
    }());
    WarriorsGuild.RankStatus = RankStatus;
    var MyRankViewModel = /** @class */ (function () {
        function MyRankViewModel() {
        }
        return MyRankViewModel;
    }());
    WarriorsGuild.MyRankViewModel = MyRankViewModel;
    var ObservableRank = /** @class */ (function () {
        function ObservableRank() {
            var _this = this;
            this.id = ko.observable(null);
            this.name = ko.observable(null);
            this.description = ko.observable(null);
            this.imageUploaded = ko.observable(null);
            this.imageExtension = ko.observable('');
            this.approvalRecordId = ko.observable(null);
            this.guardianApprovedTs = ko.observable(null);
            this.completedTs = ko.observable(null);
            this.highestPercentSubmittedForApproval = ko.observable(0);
            this.subRank = ko.observable(null);
            this.guideUploaded = ko.observable(null);
            this.percentComplete = ko.observable(0);
            this.returnReason = ko.observable(null);
            this.requirements = ko.observableArray([]);
            this.warriorCompleted = ko.pureComputed(function () {
                return _this.highestPercentSubmittedForApproval() === _this.completedPercent();
            }, this);
            this.guardianApproved = ko.pureComputed(function () {
                return _this.guardianApprovedTs() !== null;
            }, this);
            this.pendingApproval = ko.pureComputed(function () {
                return _this.approvalRecordId() !== null && !_this.guardianApproved();
            }, this);
            this.requestPromotionEnabled = ko.pureComputed(function () {
                return (_this.highestPercentSubmittedForApproval() < 100 && _this.completedPercent() === 100)
                    || _this.completedPercent() - 33 >= (_this.highestPercentSubmittedForApproval() >= 66 ? 66 : _this.highestPercentSubmittedForApproval() >= 33 ? 33 : 0);
            }, this);
            this.hasImage = ko.pureComputed(function () {
                return _this.imageUploaded() != null;
            }, this);
            this.hasGuide = ko.pureComputed(function () {
                return _this.guideUploaded() !== null;
            }, this);
            this.completedPercent = ko.pureComputed(function () {
                var total = 0;
                for (var _i = 0, _a = _this.requirements(); _i < _a.length; _i++) {
                    var req = _a[_i];
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
            this.imgSrcAttr = ko.pureComputed(function () { return _this.hasImage() ? '/images/ranks/' + _this.id() + _this.imageExtension() + '?' + _this.imageUploaded().getTime() : '/images/logo/Warriors-Guild-icon-sm-wide.png'; }, this);
            this.getDetailLink = ko.pureComputed(function () { return "/ranks#detail?id=".concat(_this.id()); });
            var self = this;
            this.requestPromotionEnabled.subscribe(function (newValue) {
                if (newValue && !self.pendingApproval() && !_this.warriorCompleted() && self.returnReason() == null) {
                    $('#requestPromotionPopup').modal('show');
                }
            });
            this.pendingApproval.subscribe(function (newValue) {
                if (newValue) {
                    $('#requestPromotionPopup').modal('hide');
                }
            });
        }
        ObservableRank.prototype.addRequirement = function (data) {
            data.requirements.push(new ObservableRankRequirement());
        };
        ;
        return ObservableRank;
    }());
    WarriorsGuild.ObservableRank = ObservableRank;
    var RankRequirement = /** @class */ (function (_super) {
        __extends(RankRequirement, _super);
        function RankRequirement() {
            var _this = _super !== null && _super.apply(this, arguments) || this;
            _this.visible = true;
            return _this;
        }
        return RankRequirement;
    }(WarriorsGuild.BaseRequirement));
    WarriorsGuild.RankRequirement = RankRequirement;
    var ObservableRankRequirement = /** @class */ (function () {
        function ObservableRankRequirement() {
            var _this = this;
            this.id = '';
            this.rankId = '';
            this.weight = ko.observable(0);
            this.actionToComplete = ko.observable('');
            this.warriorCompletedTs = ko.observable(null);
            this.guardianReviewedTs = ko.observable(null);
            this.requireAttachment = ko.observable(false);
            this.requireRing = ko.observable(false);
            this.requireCross = ko.observable(false);
            this.requiredCrossCount = ko.observable(null);
            this.savedCrosses = ko.observableArray([]);
            this.requiredRingType = ko.observable(null);
            this.requiredRingCount = ko.observable(null);
            this.savedRings = ko.observableArray([]);
            this.optional = ko.observable(false);
            this.initiatedByGuardian = ko.observable(false);
            this.showAtPercent = ko.observable(0);
            this.visible = ko.observable(true);
            this.crossesToComplete = ko.observableArray([]);
            this.totalPercentComplete = ko.observable(0);
            this.selectedCrosses = ko.observableArray([]);
            this.selectedRings = ko.observableArray([]);
            this.attachments = ko.observableArray([]);
            this.seeHowLink = ko.observable(null);
            this.seeHowLinkInvalid = ko.pureComputed(function () { return _this.seeHowLink() !== null && _this.seeHowLink().length !== 0 && !WarriorsGuild.isValidUrl(_this.seeHowLink()); }, this);
            this.actionToCompleteLinked = ko.pureComputed(function () { var _a; return (_a = _this.actionToComplete()) === null || _a === void 0 ? void 0 : _a.replace(/\n/g, '<br />').replace(/\[link ([a-zA-Z ]+)\]/g, "<a href='".concat(_this.seeHowLink(), "' target='_blank'>$1</a>")); });
            this.formatDateTime = function (date) {
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
            this.formattedWarriorCompletedTs = ko.computed(function () {
                return _this.formatDateTime(_this.warriorCompletedTs);
            }, this);
            this.formattedGuardianReviewedTs = ko.computed(function () {
                return _this.formatDateTime(_this.guardianReviewedTs);
            }, this);
            this.warriorCompleted = ko.computed(function () {
                return _this.warriorCompletedTs ? _this.warriorCompletedTs() !== null : false;
            }, this);
            this.guardianReviewed = ko.computed(function () {
                return _this.guardianReviewedTs ? _this.guardianReviewedTs() !== null : false;
            }, this);
            this.completionSummaryText = ko.computed(function () {
                var result = '';
                if (_this.warriorCompleted()) {
                    result += 'Completed at ' + _this.formattedWarriorCompletedTs();
                    if (_this.guardianReviewed())
                        result += ';  Confirmed at ' + _this.formattedGuardianReviewedTs();
                }
                return result;
            }, this);
            this.shouldBeDisabled = ko.computed(function () {
                return _this.guardianReviewed(); // || ( /*confirmationPending*/ false && !this.warriorCompleted() );
            }, this);
            this.addAttachment = function (attIds) {
                $.each(attIds, function (index, att) {
                    _this.attachments.push({ id: ko.observable(att) });
                });
            };
            var self = this;
            if (this.requireAttachment()) {
                this.warriorCompletedTs.subscribe(function (d) {
                    if (d !== null) {
                        return false;
                    }
                });
            }
            this.requireCross.subscribe(function (d) {
                if (d) {
                    _this.requiredCrossCount(1);
                }
                else {
                    _this.requiredCrossCount(0);
                }
            });
            this.requiredCrossCount.subscribe(function (d) {
                var existing = self.crossesToComplete().slice(0, d);
                self.crossesToComplete(existing);
                var itemsToConcat = d - existing.length;
                for (var i = 0; i < itemsToConcat; i++) {
                    self.crossesToComplete.push(new MinimumCrossDetail());
                }
            });
        }
        return ObservableRankRequirement;
    }());
    WarriorsGuild.ObservableRankRequirement = ObservableRankRequirement;
    var PendingRankApproval = /** @class */ (function () {
        function PendingRankApproval() {
            this.approvalRecordId = '';
            this.rankId = '';
            this.rankName = '';
            this.percentComplete = 0;
            this.rankImageUploaded = null;
            this.unconfirmedRequirements = new RankRequirement[0];
            this.returnedTs = null;
        }
        return PendingRankApproval;
    }());
    WarriorsGuild.PendingRankApproval = PendingRankApproval;
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
    var MinimumGoalDetail = /** @class */ (function () {
        function MinimumGoalDetail() {
            this.name = '';
            this.imageExtension = '';
            this.hasImage = false;
            this.imgSrcAttr = '';
            this.percentCompleted = 0;
        }
        return MinimumGoalDetail;
    }());
    WarriorsGuild.MinimumGoalDetail = MinimumGoalDetail;
    var MinimumRingDetail = /** @class */ (function (_super) {
        __extends(MinimumRingDetail, _super);
        function MinimumRingDetail() {
            var _this = _super !== null && _super.apply(this, arguments) || this;
            _this.ringId = '';
            _this.type = '';
            return _this;
        }
        return MinimumRingDetail;
    }(MinimumGoalDetail));
    WarriorsGuild.MinimumRingDetail = MinimumRingDetail;
    var MinimumCrossDetail = /** @class */ (function (_super) {
        __extends(MinimumCrossDetail, _super);
        function MinimumCrossDetail() {
            var _this = _super.call(this) || this;
            _this.crossId = ko.observable('');
            return _this;
        }
        return MinimumCrossDetail;
    }(MinimumGoalDetail));
    WarriorsGuild.MinimumCrossDetail = MinimumCrossDetail;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=ranks.datamodels.js.map