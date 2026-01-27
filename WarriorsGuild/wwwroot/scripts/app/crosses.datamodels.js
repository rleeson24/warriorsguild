var crossSummaryQuestionsWeight = 10;
var WarriorsGuild;
(function (WarriorsGuild) {
    var Cross = /** @class */ (function () {
        function Cross() {
        }
        return Cross;
    }());
    WarriorsGuild.Cross = Cross;
    var PinnedCross = /** @class */ (function () {
        function PinnedCross() {
        }
        return PinnedCross;
    }());
    WarriorsGuild.PinnedCross = PinnedCross;
    var ObservableCross = /** @class */ (function () {
        function ObservableCross() {
            var _this = this;
            this.id = ko.observable('');
            this.name = ko.observable('');
            this.description = ko.observable('');
            this.explainText = ko.observable('');
            this.imageUploaded = ko.observable(null);
            this.imageExtension = ko.observable('');
            this.questions = ko.observableArray([]);
            this.days = ko.observableArray([]);
            this.guideUploaded = ko.observable(null);
            this.isPinned = ko.observable(false);
            this.isCompleted = ko.observable(false);
            this.explainTextPreview = ko.pureComputed(function () {
                var _a;
                if (_this.questions().length > 0) {
                    for (var i = 0; i < _this.questions().length; i++) {
                        var qText = _this.questions()[i].text;
                        if (qText.indexOf('{explain}') > -1) {
                            return qText.replace('{explain}', (_a = _this.explainText()) !== null && _a !== void 0 ? _a : '[nothing entered]');
                        }
                    }
                }
                else {
                    return '[nothing entered]';
                }
                return "oops";
            }, this);
            this.hasImage = ko.pureComputed(function () {
                return _this.imageUploaded() != null;
            }, this);
            this.addDay = function () {
                var oDay = new ObservableCrossDay();
                _this.days.push(oDay);
            };
            this.imgSrcAttr = ko.pureComputed(function () { return _this.hasImage() ? '/images/crosses/' + _this.id() + _this.imageExtension() + '?' + _this.imageUploaded().getTime() : '/images/logo/Warriors-Guild-icon-sm-wide.png'; }, this);
            this.getDetailLink = ko.pureComputed(function () { return "/crosses#detail?id=".concat(_this.id()); });
            this.completedAt = ko.observable(null);
            this.approvedAt = ko.observable(null);
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
                return _this.formatDateTime(_this.completedAt);
            });
            this.formattedGuardianReviewedTs = ko.computed(function () {
                return _this.formatDateTime(_this.approvedAt);
            });
            this.warriorCompleted = ko.computed(function () {
                return _this.completedAt ? _this.completedAt() !== null : false;
            });
            this.guardianReviewed = ko.computed(function () {
                return _this.approvedAt ? _this.approvedAt() !== null : false;
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
            this.hasGuide = ko.pureComputed(function () {
                return _this.guideUploaded() !== null;
            }, this);
            this.guideUploaderSettings = ko.pureComputed(function () {
                return {
                    key: 'uploadRankDocumentation',
                    id: _this.id(),
                    postUrl: crossUrls.uploadGuideUrl + '/' + _this.id(),
                    imgUrl: crossUrls.downloadGuideUrl + '/' + _this.id(),
                    fileClass: 'pdf',
                    successCallBack: function () { _this.guideUploaded(new Date()); }
                };
            }, this);
            this.downloadGuideUrl = ko.pureComputed(function () { return crossUrls.downloadGuideUrl + '/' + _this.id(); }, this);
        }
        ObservableCross.prototype.addQuestion = function (data) {
            data.questions.push(new ObservableCrossQuestion());
        };
        ;
        ObservableCross.prototype.createObservableCrossQuestion = function (arg0) {
            var q = new ObservableCrossQuestion();
            q.text = arg0;
            return q;
        };
        return ObservableCross;
    }());
    WarriorsGuild.ObservableCross = ObservableCross;
    var CrossQuestion = /** @class */ (function () {
        function CrossQuestion() {
        }
        return CrossQuestion;
    }());
    WarriorsGuild.CrossQuestion = CrossQuestion;
    var ObservableCrossQuestion = /** @class */ (function () {
        function ObservableCrossQuestion() {
            this.id = '';
            this.crossId = '';
            this.showEntryField = true;
            this.text = '';
            this.answer = ko.observable('');
        }
        return ObservableCrossQuestion;
    }());
    WarriorsGuild.ObservableCrossQuestion = ObservableCrossQuestion;
    var CrossDay = /** @class */ (function () {
        function CrossDay() {
        }
        return CrossDay;
    }());
    WarriorsGuild.CrossDay = CrossDay;
    var ObservableCrossDay = /** @class */ (function () {
        function ObservableCrossDay() {
            var _this = this;
            this.id = null;
            this.passage = ko.observable('');
            this.weight = ko.observable(0);
            this.index = ko.observable(0);
            this.isCheckpoint = ko.observable(false);
            this.questions = ko.observableArray([]);
            this.completedAt = ko.observable(null);
            this.approvedAt = ko.observable(null);
            this.warriorCompleted = ko.computed(function () {
                return _this.completedAt ? _this.completedAt() !== null : false;
            });
            this.guardianReviewed = ko.computed(function () {
                return _this.approvedAt ? _this.approvedAt() !== null : false;
            });
            this.editing = ko.observable(false);
        }
        return ObservableCrossDay;
    }());
    WarriorsGuild.ObservableCrossDay = ObservableCrossDay;
    var PendingCrossApproval = /** @class */ (function () {
        function PendingCrossApproval() {
            this.approvalRecordId = '';
            this.crossId = '';
            this.crossName = '';
            this.percentComplete = 0;
            this.crossImageUploaded = ko.observable(null);
            //unconfirmedRequirements: RankRequirement[] = new RankRequirement[0];
            this.imageExtension = '';
        }
        return PendingCrossApproval;
    }());
    WarriorsGuild.PendingCrossApproval = PendingCrossApproval;
    var CrossQuestionAnswer = /** @class */ (function () {
        function CrossQuestionAnswer() {
        }
        return CrossQuestionAnswer;
    }());
    WarriorsGuild.CrossQuestionAnswer = CrossQuestionAnswer;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=crosses.datamodels.js.map