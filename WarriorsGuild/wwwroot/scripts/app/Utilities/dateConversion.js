var WarriorsGuild;
(function (WarriorsGuild) {
    var DateConversion = /** @class */ (function () {
        function DateConversion() {
        }
        DateConversion.convertStringToNullableDate = function (dateString) {
            if (!dateString)
                return null;
            var d = new Date(dateString);
            var date = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate(), d.getHours(), d.getMinutes(), d.getSeconds());
            return new Date(date);
        };
        return DateConversion;
    }());
    WarriorsGuild.DateConversion = DateConversion;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=dateConversion.js.map