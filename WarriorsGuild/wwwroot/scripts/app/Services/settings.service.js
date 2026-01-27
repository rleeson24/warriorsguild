var WarriorsGuild;
(function (WarriorsGuild) {
    var SettingsService = /** @class */ (function () {
        function SettingsService() {
            var _this = this;
            this.crossStatusUrl = '/api/crossstatus';
            this.crossUrl = '/api/crosses';
            this.getCrossDetail = function (crossId, success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/").concat(crossId),
                    contentType: "application/json; charset=utf-8",
                    success: success
                });
            };
            this.retrieveCrossQuestions = function (crossId, success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/").concat(crossId, "/questions"),
                    contentType: "application/json; charset=utf-8",
                    success: success
                });
            };
            this.getCrossDays = function (crossId, success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/").concat(crossId, "/days"),
                    contentType: "application/json; charset=utf-8",
                    success: success
                });
            };
            this.saveAnswers = function (crossId, answers, success, error) {
                WarriorsGuild.serviceBase.put({
                    url: "".concat(_this.crossUrl, "/").concat(crossId, "/answers"),
                    data: ko.toJSON(answers),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.completeCross = function (crossId, answers, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(crossId, "/complete"),
                    data: ko.toJSON(answers),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.confirmCrossComplete = function (approvalRecordId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(approvalRecordId, "/confirmComplete"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.confirmCrossCheckpointComplete = function (approvalRecordId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(approvalRecordId, "/confirmComplete"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.returnCross = function (crossId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(crossId, "/return?reason=test"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.getPendingApprovals = function (crossId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossStatusUrl, "/pendingapprovalsbycross/").concat(crossId),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.getUnassignedCrosses = function (success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/unassigned"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
            this.getCompletedCrosses = function (warriorId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/byuser/").concat(warriorId, "/completed"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
        }
        return SettingsService;
    }());
    WarriorsGuild.SettingsService = SettingsService;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=settings.service.js.map