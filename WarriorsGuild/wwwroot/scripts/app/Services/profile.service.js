var WarriorsGuild;
(function (WarriorsGuild) {
    var ProfileService = /** @class */ (function () {
        function ProfileService() {
            var _this = this;
            this.profileUrl = '/api/profile';
            this.retrieveProfileData = function (userId, success) {
                WarriorsGuild.serviceBase.get({
                    url: "".concat(_this.profileUrl, "/").concat(userId),
                    success: success
                });
            };
            this.setWarriorProfile = function (warriorId) {
                WarriorsGuild.serviceBase.post({
                    url: '/api/Profile/SetActiveWarrior',
                    data: "\"".concat(warriorId, "\""),
                });
            };
            this.togglePreviewMode = function (success, error) {
                WarriorsGuild.serviceBase.post({
                    url: '/api/Profile/TogglePreviewMode',
                    success: success,
                    error: error
                });
            };
        }
        return ProfileService;
    }());
    WarriorsGuild.ProfileService = ProfileService;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=profile.service.js.map