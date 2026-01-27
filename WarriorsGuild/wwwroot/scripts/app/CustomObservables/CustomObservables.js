/*!currencyObservable*/
/*globals ko, jQuery*/
var WarriorsGuild;
(function (WarriorsGuild) {
    var KnockoutCustomObservables = /** @class */ (function () {
        function KnockoutCustomObservables() {
            this.currencyComputed = (function () {
                return (function ($) {
                    var cleanInput = function (value) {
                        if (value !== undefined && value !== null) {
                            value = value.toString();
                            value = value.replace('$', '');
                            var floatValue = parseFloat(value);
                            return floatValue;
                        }
                        return 0;
                    }, format = function (value) {
                        try {
                            var parts = value.toFixed(2).split(".");
                            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                            return '$' + parts.join(".");
                        }
                        catch (ex) {
                            return '';
                        }
                    };
                    return function (initialValue) {
                        var raw = typeof initialValue === "function"
                            ? ko.pureComputed(initialValue, arguments[1])
                            : ko.observable(cleanInput(initialValue));
                        var publicFn = ko.computed({
                            read: function () { return raw(); },
                            write: function (value) {
                                var input = cleanInput(value);
                                raw(input);
                            }
                        }, this);
                        publicFn["formatted"] = ko.computed({
                            read: function () { return format(raw()); }
                            //, write: function ( value ) { raw( cleanInput( value ) ); }
                        });
                        publicFn["type"] = 'currencyObservable';
                        return publicFn;
                    };
                }(jQuery));
            })();
            this.numericComputed = (function () {
                return (function ($) {
                    return function (initialValue) {
                        var _actual = ko.observable(initialValue);
                        var result = ko.computed({
                            read: function () {
                                return _actual();
                            },
                            write: function (newValue) {
                                var parsedValue = parseFloat(newValue);
                                _actual(isNaN(parsedValue) ? newValue : parsedValue);
                            }
                        });
                        return result;
                    };
                }(jQuery));
            })();
            this.phoneComputed = (function () {
                return (function ($) {
                    var cleanInput = function (value) {
                        return value ? value.replace(/[\(\)\-\_ ]/g, '') : '';
                    }, format = function (value) {
                        if (value === undefined) {
                            value = '';
                        }
                        if (value.length > 0) {
                            if (value.length > 6) {
                                return '(' + value.substr(0, 3) + ') ' + value.substr(3, 3) + '-' + value.substr(6);
                            }
                            else if (value.length > 3) {
                                return '(' + value.substr(0, 3) + ') ' + value.substr(3);
                            }
                            else {
                                return '(' + (value + '   ').substr(0, 3) + ')';
                            }
                        }
                        return '';
                    };
                    return function (initialValue) {
                        var raw = typeof initialValue === "function" ?
                            ko.pureComputed(initialValue) : ko.observable(initialValue);
                        var publicFn = ko.computed({
                            read: function () { return raw(); },
                            write: function (value) { raw(cleanInput(value)); }
                        });
                        publicFn["formatted"] = ko.computed({
                            read: function () { return format(raw()); },
                            write: function (value) { raw(cleanInput(value)); }
                        });
                        publicFn["type"] = 'phoneObservable';
                        return publicFn;
                    };
                }(jQuery));
            })();
        }
        return KnockoutCustomObservables;
    }());
    WarriorsGuild.KnockoutCustomObservables = KnockoutCustomObservables;
})(WarriorsGuild || (WarriorsGuild = {}));
window['koCustom'] = new WarriorsGuild.KnockoutCustomObservables();
//# sourceMappingURL=CustomObservables.js.map