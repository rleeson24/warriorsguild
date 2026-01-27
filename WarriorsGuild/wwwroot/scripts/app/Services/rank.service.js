var WarriorsGuild;
(function (WarriorsGuild) {
    var RankService = /** @class */ (function () {
        function RankService() {
            var _this = this;
            this.rankUrl = '/api/ranks';
            this.rankStatusUrl = '/api/rankstatus';
            this.getRanks = function (success, error) {
                WarriorsGuild.serviceBase.get({
                    url: _this.rankUrl,
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.getPublicRank = function (success, error) {
                WarriorsGuild.serviceBase.get({
                    url: rankUrls.publicRankUrl,
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.getRankDetail = function (rankId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: _this.rankUrl + '/' + rankId,
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.retrieveRankRequirements = function (rankId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.rankUrl, "/").concat(rankId, "/requirements"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.getPendingApprovals = function (rankId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.rankStatusUrl, "/approvalsForRank/").concat(rankId),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.markRequirementComplete = function (rankId, rankRequirementId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.rankStatusUrl, "/RecordCompletion"),
                    data: ko.toJSON({ RankRequirementId: rankRequirementId, RankId: rankId }),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.markRequirementCompleteByGuardian = function (rankId, rankRequirementId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.rankStatusUrl, "/RecordCompletionAsGuardian"),
                    data: ko.toJSON({ RankRequirementId: rankRequirementId, RankId: rankId }),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.create = function (model, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: _this.rankUrl,
                    data: ko.toJSON(model),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.update = function (model, success, error) {
                WarriorsGuild.serviceBase.put({
                    url: "".concat(_this.rankUrl, "/").concat(model.Id),
                    data: ko.toJSON(model),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.saveNewRankOrder = function (rankOrder, success, error) {
                WarriorsGuild.serviceBase.post({
                    async: false,
                    timeout: 5000,
                    url: "".concat(rankUrls.ranksUrl, "/Order"),
                    data: ko.toJSON(rankOrder),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.returnToWarrior = function (approvalRecordId, reasonForReturn, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.rankStatusUrl, "/").concat(approvalRecordId, "/return?reason=").concat(reasonForReturn),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.confirmRankCompletion = function (approvalRecordId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.rankStatusUrl, "/").concat(approvalRecordId, "/ApproveProgress"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
        }
        return RankService;
    }());
    WarriorsGuild.RankService = RankService;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=rank.service.js.map