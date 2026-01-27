var WarriorsGuild;
(function (WarriorsGuild) {
    var CrossService = /** @class */ (function () {
        function CrossService() {
            var _this = this;
            this.crossStatusUrl = '/api/crossstatus';
            this.crossUrl = '/api/crosses';
            this.getCrossList = function (success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl),
                    success: success
                });
            };
            this.getPublicCross = function (success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/public"),
                    success: success
                });
            };
            this.getCrossDetail = function (crossId, success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/").concat(crossId),
                    success: success
                });
            };
            this.retrieveCrossQuestions = function (crossId, success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/").concat(crossId, "/questions"),
                    success: success
                });
            };
            this.retrieveTemplateQuestions = function (success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/questionsByTemplate/dayquestions"),
                    success: success
                });
            };
            this.getCrossDays = function (crossId, success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/").concat(crossId, "/days"),
                    success: success
                });
            };
            this.createCross = function (crossDetail, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: _this.crossUrl,
                    data: ko.toJSON(crossDetail),
                    success: success,
                    error: error
                });
            };
            this.updateCross = function (crossDetail, success, error) {
                WarriorsGuild.serviceBase.put({
                    url: "".concat(_this.crossUrl, "/").concat(crossDetail.Id),
                    data: ko.toJSON(crossDetail),
                    success: success,
                    error: error
                });
            };
            this.savePassageAnswers = function (crossId, dayId, answers, success, error) {
                WarriorsGuild.serviceBase.put({
                    url: "".concat(_this.crossUrl, "/").concat(crossId, "/day/").concat(dayId, "/answers"),
                    data: ko.toJSON(answers),
                    success: success,
                    error: error
                });
            };
            this.saveSummaryAnswers = function (crossId, answers, success, error) {
                WarriorsGuild.serviceBase.put({
                    url: "".concat(_this.crossUrl, "/").concat(crossId, "/answers"),
                    data: ko.toJSON(answers),
                    success: success,
                    error: error
                });
            };
            this.completeCrossDay = function (crossId, dayId, answers, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(crossId, "/day/").concat(dayId, "/complete"),
                    data: ko.toJSON(answers),
                    success: success,
                    error: error
                });
            };
            this.completeCross = function (crossId, answers, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(crossId, "/complete"),
                    data: ko.toJSON(answers),
                    success: success,
                    error: error
                });
            };
            this.confirmCrossComplete = function (approvalRecordId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(approvalRecordId, "/confirmComplete"),
                    success: success,
                    error: error
                });
            };
            this.confirmCrossCheckpointComplete = function (approvalRecordId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(approvalRecordId, "/confirmComplete"),
                    success: success,
                    error: error
                });
            };
            this.returnCross = function (crossId, success, error) {
                WarriorsGuild.serviceBase.post({
                    url: "".concat(_this.crossStatusUrl, "/").concat(crossId, "/return?reason=test"),
                    success: success,
                    error: error
                });
            };
            this.getPendingApprovals = function (crossId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossStatusUrl, "/pendingapprovalsbycross/").concat(crossId),
                    success: success,
                    error: error
                });
            };
            this.getUnassignedCrosses = function (success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/unassigned"),
                    success: success,
                    error: error
                });
            };
            this.getCompletedCrossesForUser = function (warriorId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/byuser/").concat(warriorId, "/completed"),
                    success: success,
                    error: error
                });
            };
            this.getCompletedCrosses = function (success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/completed"),
                    success: success,
                    error: error
                });
            };
            this.getPinnedCrosses = function (success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.crossUrl, "/pinned"),
                    success: success,
                    error: error
                });
            };
            this.updateCrossOrder = function (crossOrder, success, error) {
                WarriorsGuild.serviceBase.post({
                    async: false,
                    timeout: 5000,
                    url: "".concat(_this.crossUrl, "/Order"),
                    data: ko.toJSON(crossOrder),
                    error: error,
                    success: success
                });
            };
        }
        CrossService.prototype.updatePassages = function (crossId, daysToSave, success, error) {
            WarriorsGuild.serviceBase.put({
                method: 'put',
                url: "".concat(this.crossUrl, "/").concat(crossId, "/days"),
                data: ko.toJSON(daysToSave),
                success: success,
                error: error
            });
        };
        CrossService.prototype.confirmCrossDayComplete = function (approvalRecordId, success, error) {
            WarriorsGuild.serviceBase.post({
                url: "".concat(this.crossStatusUrl, "/").concat(approvalRecordId),
                success: success,
                error: error
            });
        };
        CrossService.prototype.pinCross = function (crossId, success, error) {
            WarriorsGuild.serviceBase.post({
                url: "".concat(this.crossUrl, "/pin/").concat(crossId),
                success: success,
                error: error
            });
        };
        CrossService.prototype.unpinCross = function (crossId, success, error) {
            WarriorsGuild.serviceBase.post({
                url: "".concat(this.crossUrl, "/unpin/").concat(crossId),
                success: success,
                error: error
            });
        };
        return CrossService;
    }());
    WarriorsGuild.CrossService = CrossService;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=cross.service.js.map