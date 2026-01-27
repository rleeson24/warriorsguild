namespace WarriorsGuild {
    export class KnockoutMapperConfigurations {
        static koCrossMapperConfiguration = (() => {
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
    }
}