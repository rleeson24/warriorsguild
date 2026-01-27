var WarriorsGuild;
(function (WarriorsGuild) {
    var KnockoutMapperConfigurations = /** @class */ (function () {
        function KnockoutMapperConfigurations() {
        }
        KnockoutMapperConfigurations.koCrossMapperConfiguration = (function () {
            return {
                'completedAt': {
                    update: function (options) {
                        return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                    }
                },
                'approvedAt': {
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
                }
            };
        })();
        return KnockoutMapperConfigurations;
    }());
    WarriorsGuild.KnockoutMapperConfigurations = KnockoutMapperConfigurations;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=mapperConfigurations.js.map