var WarriorsGuild;
(function (WarriorsGuild) {
    var common = /** @class */ (function () {
        function common() {
        }
        common.getFragment = function () {
            if (window.location.hash.indexOf("#") === 0 && window.location.hash !== '#_=_') {
                return common.parseQueryString(window.location.hash.substr(1));
            }
            else {
                return {};
            }
        };
        common.parseQueryString = function (queryString) {
            var data = {}, pairs, pair, separatorIndex, escapedKey, escapedValue, key, value;
            if (queryString === null) {
                return data;
            }
            pairs = queryString.split("&");
            for (var i = 0; i < pairs.length; i++) {
                pair = pairs[i];
                separatorIndex = pair.indexOf("=");
                if (separatorIndex === -1) {
                    escapedKey = pair;
                    escapedValue = null;
                }
                else {
                    escapedKey = pair.substr(0, separatorIndex);
                    escapedValue = pair.substr(separatorIndex + 1);
                }
                key = decodeURIComponent(escapedKey);
                value = decodeURIComponent(escapedValue);
                data[key] = value;
            }
            return data;
        };
        return common;
    }());
    WarriorsGuild.common = common;
    var BaseRequirement = /** @class */ (function () {
        function BaseRequirement() {
        }
        return BaseRequirement;
    }());
    WarriorsGuild.BaseRequirement = BaseRequirement;
    function ParseResponseErrorWithLeadingPeriod(err) {
        var response = ParseResponseError(err);
        return response > '' ? ". ".concat(response) : '';
    }
    WarriorsGuild.ParseResponseErrorWithLeadingPeriod = ParseResponseErrorWithLeadingPeriod;
    function ParseResponseError(err) {
        if (err.responseJSON) {
            if (err.responseJSON.message) {
                return err.responseJSON.message;
            }
            else {
                return err.responseJSON.title;
            }
        }
        else {
            return !err.responseText ? '' : err.responseText;
        }
    }
    WarriorsGuild.ParseResponseError = ParseResponseError;
    function isValidUrl(inputString) {
        var expression = /[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)?/gi;
        var regex = new RegExp(expression);
        return regex.test(inputString);
    }
    WarriorsGuild.isValidUrl = isValidUrl;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=common.js.map