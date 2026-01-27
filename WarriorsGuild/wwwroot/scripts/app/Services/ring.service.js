var WarriorsGuild;
(function (WarriorsGuild) {
    var RingService = /** @class */ (function () {
        function RingService() {
            var _this = this;
            this.ringUrl = '/api/rings';
            this.ringStatusUrl = '/api/ringstatus';
            this.getCompletedRings = function (warriorId, success, error) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.ringUrl, "/byuser/").concat(warriorId, "/completed"),
                    contentType: "application/json; charset=utf-8",
                    success: success,
                    error: error
                });
            };
        }
        return RingService;
    }());
    WarriorsGuild.RingService = RingService;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=ring.service.js.map