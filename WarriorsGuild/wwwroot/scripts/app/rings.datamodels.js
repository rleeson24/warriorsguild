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
    var Ring = /** @class */ (function () {
        function Ring() {
        }
        return Ring;
    }());
    WarriorsGuild.Ring = Ring;
    var PinnedRing = /** @class */ (function () {
        function PinnedRing() {
        }
        return PinnedRing;
    }());
    WarriorsGuild.PinnedRing = PinnedRing;
    var RingStatus = /** @class */ (function () {
        function RingStatus() {
        }
        return RingStatus;
    }());
    WarriorsGuild.RingStatus = RingStatus;
    var ObservableRing = /** @class */ (function () {
        function ObservableRing() {
            var _this = this;
            this.id = ko.observable('');
            this.name = ko.observable('');
            this.description = ko.observable('');
            this.type = ko.observable('');
            this.imageUploaded = ko.observable(null);
            this.imageExtension = ko.observable('');
            this.requirements = ko.observableArray([]);
            this.approvalRecordId = ko.observable(null);
            this.guardianApprovedTs = ko.observable(null);
            this.isPinned = ko.observable(false);
            this.isCompleted = ko.observable(false);
            this.warriorCompletedTs = ko.observable(null);
            this.guardianReviewedTs = ko.observable(null);
            this.guideUploaded = ko.observable(null);
            this.warriorCompleted = ko.computed(function () {
                return _this.warriorCompletedTs ? _this.warriorCompletedTs() !== null : false;
            });
            this.guardianReviewed = ko.computed(function () {
                return _this.guardianReviewedTs ? _this.guardianReviewedTs() !== null : false;
            });
            this.hasImage = ko.pureComputed(function () {
                return _this.imageUploaded() != null;
            }, this);
            this.pendingApproval = ko.pureComputed(function () {
                return _this.approvalRecordId() !== null && !_this.guardianReviewed();
            }, this);
            this.completedPercent = ko.pureComputed(function () {
                var total = 0;
                for (var i = 0; i < _this.requirements().length; i++) {
                    if (_this.requirements()[i].warriorCompleted()) {
                        total += _this.requirements()[i].weight();
                    }
                }
                return total;
            }, this);
            this.requestPromotionEnabled = ko.pureComputed(function () {
                return _this.completedPercent() === 100 && !_this.warriorCompleted();
            }, this);
            this.hasGuide = ko.pureComputed(function () {
                return _this.guideUploaded() !== null;
            }, this);
            this.imgSrcAttr = ko.pureComputed(function () { return _this.hasImage() ? '/images/rings/' + _this.id() + _this.imageExtension() + '?' + _this.imageUploaded().getTime() : '/images/logo/Warriors-Guild-icon-sm-wide.png'; }, this);
            this.getDetailLink = ko.pureComputed(function () { return "/rings#detail?id=".concat(_this.id()); });
            var self = this;
            this.requestPromotionEnabled.subscribe(function (newValue) {
                if (newValue && !self.pendingApproval()) {
                    $('#requestPromotionPopup').modal('show');
                }
            });
            this.pendingApproval.subscribe(function (newValue) {
                if (newValue) {
                    $('#requestPromotionPopup').modal('hide');
                }
            });
        }
        ObservableRing.prototype.addRequirement = function (data) {
            data.requirements.push(new ObservableRingRequirement());
        };
        ;
        return ObservableRing;
    }());
    WarriorsGuild.ObservableRing = ObservableRing;
    var RingRequirement = /** @class */ (function (_super) {
        __extends(RingRequirement, _super);
        function RingRequirement() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        return RingRequirement;
    }(WarriorsGuild.BaseRequirement));
    WarriorsGuild.RingRequirement = RingRequirement;
    var ObservableRingRequirement = /** @class */ (function () {
        function ObservableRingRequirement() {
            var _this = this;
            this.id = '';
            this.ringId = '';
            this.weight = ko.observable(0);
            this.actionToComplete = ko.observable('');
            this.warriorCompletedTs = ko.observable(null);
            this.guardianReviewedTs = ko.observable(null);
            this.requireAttachment = ko.observable(false);
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
            });
            this.formattedGuardianReviewedTs = ko.computed(function () {
                return _this.formatDateTime(_this.guardianReviewedTs);
            });
            this.warriorCompleted = ko.computed(function () {
                return _this.warriorCompletedTs ? _this.warriorCompletedTs() !== null : false;
            });
            this.guardianReviewed = ko.computed(function () {
                return _this.guardianReviewedTs ? _this.guardianReviewedTs() !== null : false;
            });
            this.completionSummaryText = ko.computed(function () {
                var result = '';
                if (_this.warriorCompleted()) {
                    result += 'Completed at ' + _this.formattedWarriorCompletedTs();
                    if (_this.guardianReviewed())
                        result += ';  Confirmed at ' + _this.formattedGuardianReviewedTs();
                }
                return result;
            }, this);
            this.addAttachment = function (attIds) {
                $.each(attIds, function (index, att) {
                    _this.attachments.push({ id: ko.observable(att) });
                });
            };
            if (this.requireAttachment()) {
                this.warriorCompletedTs.subscribe(function (d) {
                    if (d !== null) {
                        return false;
                    }
                });
            }
        }
        return ObservableRingRequirement;
    }());
    WarriorsGuild.ObservableRingRequirement = ObservableRingRequirement;
    var PendingRingApproval = /** @class */ (function () {
        function PendingRingApproval() {
            this.approvalRecordId = '';
            this.ringId = '';
            this.ringName = '';
            this.percentComplete = 0;
            this.ringImageUploaded = ko.observable(null);
        }
        return PendingRingApproval;
    }());
    WarriorsGuild.PendingRingApproval = PendingRingApproval;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=rings.datamodels.js.map