(function (g, l, t) {
    var z;
    var s = new m;
    var e = "5.1.28";
    var p = {
        candidates: 3,
        autocomplete: 10,
        requestUrlInternational: "https://international-street.api.smartystreets.com/verify",
        requestUrlUS: "https://us-street.api.smartystreets.com/street-address",
        timeout: 5000,
        speed: "medium",
        ambiguousMessage: "Matched multiple addresses.<br>which did you mean?",
        invalidMessage: "You entered an unknown address:",
        invalidCountryMessage: "Unknown country",
        missingSecondaryMessage: "You forgot your apt/suite number or entered an unknown apt/suite number",
        certifyMessage: "Use as it is",
        missingInputMessage: "You didn't enter enough information",
        changeMessage: "Go back",
        noSuggestionsMessage: "No suggestions",
        fieldSelector: "input[type=text], input:not([type]), textarea, select",
        submitSelector: "[type=submit], [type=image], [type=button]:last, button:last",
        target: "US",
        preferRatio: 0.333333333,
        ajaxSettings: {},
        smartyTag: true
    };
    var A = {};
    var d = [];
    var B = "body";
    var E = 0;
    var f = ["freeform", "address1", "address2", "address3", "address4", "organization", "locality", "administrative_area", "postal_code", "country", "match"];
    g.LiveAddress = function (F) {
        return g(B).LiveAddress(F);
    };
    g.fn.LiveAddress = function (F) {
        var G = g.fn.jquery.split(".");
        if (G.length >= 2) {
            if (G[0] < 1 || (G[0] == 1 && G[1] < 5)) {
                console.log("jQuery version " + g.fn.jquery + " found, but LiveAddress requires jQuery version 1.5 or higher. Aborting.");
                return false;
            }
        }
        else {
            return false;
        }
        if (F.debug) {
            console.log("LiveAddress API jQuery Plugin version " + e + " (Debug mode)");
        }
        c();
        if (t.readyState === "complete") {
            l.loaded = true;
        }
        else {
            g(l).on("load", function () {
                l.loaded = true;
            });
        }
        if (typeof F === "string") {
            A = {
                key: F
            };
        }
        else {
            if (typeof F === "object") {
                A = F;
            }
        }
        A.candidates = A.candidates || p.candidates;
        A.ui = typeof A.ui === "undefined" ? true : A.ui;
        A.autoVerify = A.autoVerify !== true && A.autoVerify !== false ? true : A.autoVerify;
        A.submitVerify = typeof A.submitVerify === "undefined" ? true : A.submitVerify;
        A.timeout = A.timeout || p.timeout;
        A.ambiguousMessage = A.ambiguousMessage || p.ambiguousMessage;
        A.invalidMessage = A.invalidMessage || p.invalidMessage;
        A.invalidCountryMessage = A.invalidCountryMessage || p.invalidCountryMessage;
        A.missingSecondaryMessage = A.missingSecondaryMessage || p.missingSecondaryMessage;
        A.certifyMessage = A.certifyMessage || p.certifyMessage;
        A.missingInputMessage = A.missingInputMessage || p.missingInputMessage;
        A.changeMessage = A.changeMessage || p.changeMessage;
        A.noSuggestionsMessage = A.noSuggestionsMessage || p.noSuggestionsMessage;
        A.fieldSelector = A.fieldSelector || p.fieldSelector;
        A.submitSelector = A.submitSelector || p.submitSelector;
        A.requestUrlInternational = A.requestUrlInternational || p.requestUrlInternational;
        A.requestUrlUS = A.requestUrlUS || p.requestUrlUS;
        A.autocomplete = typeof A.autocomplete === "undefined" ? p.autocomplete : A.autocomplete;
        A.cityFilter = typeof A.cityFilter === "undefined" ? "" : A.cityFilter;
        A.stateFilter = typeof A.stateFilter === "undefined" ? "" : A.stateFilter;
        A.cityStatePreference = typeof A.cityStatePreference === "undefined" ? "" : A.cityStatePreference;
        A.geolocate = typeof A.geolocate === "undefined" ? true : A.geolocate;
        A.geolocatePrecision = typeof A.geolocatePrecision === "undefined" ? "city" : A.geolocatePrecision;
        A.waitForStreet = typeof A.waitForStreet === "undefined" ? false : A.waitForStreet;
        A.verifySecondary = typeof A.verifySecondary === "undefined" ? false : A.verifySecondary;
        A.geocode = typeof A.geocode === "undefined" ? false : A.geocode;
        A.enforceVerification = typeof A.enforceVerification === "undefined" ? false : A.enforceVerification;
        A.agent = typeof A.agent === "undefined" ? "" : A.agent;
        A.preferRatio = A.preferRatio || p.preferRatio;
        A.ajaxSettings = A.ajaxSettings || p.ajaxSettings;
        A.smartyTag = typeof A.smartyTag === "undefined" ? p.smartyTag : A.smartyTag;
        if (typeof A.autocomplete === "number" && A.autocomplete < 1) {
            A.autocomplete = false;
        }
        if (!A.target || typeof A.target != "string") {
            A.target = p.target;
        }
        A.target = A.target.toUpperCase().replace(/\s+/g, "").split("|");
        z = {
            events: {
                FieldsMapped: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "FieldsMapped", "(Fields mapped to their respective addresses)", I, J);
                    }
                    l.loaded ? s.postMappingOperations() : g(s.postMappingOperations);
                },
                MapInitialized: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "MapInitialized", "(Mapped fields have been wired up to the window" + (A.ui ? ", document, and UI" : " and document") + ")", I, J);
                    }
                },
                AutocompleteInvoked: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AutocompleteInvoked", "(A request is about to be sent to the autocomplete service)", I, J);
                    }
                    s.requestAutocomplete(I, J);
                },
                AutocompleteReceived: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AutocompleteReceived", "(A response has just been received from the autocomplete service)", I, J);
                    }
                    s.showAutocomplete(I, J);
                },
                AutocompleteUsed: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AutocompleteUsed", "(A suggested address was used from the autocomplete service)", I, J);
                    }
                },
                AddressChanged: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddressChanged", "(Address changed)", I, J);
                    }
                    if (A.autoVerify && J.address.enoughInput() && (J.address.verifyCount == 0 || J.address.isFreeform() || J.address.usedAutocomplete) && !J.suppressAutoVerification && J.address.hasDomFields() && J.address.active && !J.address.autocompleteVisible() && (J.address.form && !J.address.form.processing)) {
                        r("VerificationInvoked", {
                            address: J.address
                        });
                    }
                    J.address.usedAutocomplete = false;
                },
                VerificationInvoked: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "VerificationInvoked", "(Address verification invoked)", I, J);
                    }
                    if (!J.address || (J.address && J.address.form && J.address.form.processing)) {
                        if (A.debug) {
                            console.log("NOTICE: VerificationInvoked event handling aborted. Address is missing or an address in the same form is already processing.");
                        }
                        return;
                    }
                    else {
                        if (J.address.status() == "accepted" && !J.verifyAccepted) {
                            if (A.debug) {
                                console.log("NOTICE: VerificationInvoked raised on an accepted or un-changed address. Nothing to do.");
                            }
                            return r("Completed", J);
                        }
                        else {
                            if (J.address.form) {
                                J.address.form.processing = true;
                            }
                        }
                    }
                    J.address.verify(J.invoke, J.invokeFn);
                },
                RequestSubmitted: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "RequestSubmitted", "(Request submitted to server)", I, J);
                    }
                    s.showLoader(J.address);
                },
                ResponseReceived: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "ResponseReceived", "(Response received from server, but has not been inspected)", I, J);
                    }
                    s.hideLoader(J.address);
                    if (typeof J.invoke === "function") {
                        J.invoke(J.response);
                    }
                    if (J.response.isInvalid()) {
                        r("AddressWasInvalid", J);
                    }
                    else {
                        if (J.response.isAmbiguous()) {
                            r("AddressWasAmbiguous", J);
                        }
                        else {
                            if (A.verifySecondary && J.response.isMissingSecondary()) {
                                r("AddressWasMissingSecondary", J);
                            }
                            else {
                                if (J.response.isValid()) {
                                    r("AddressWasValid", J);
                                }
                            }
                        }
                    }
                },
                RequestTimedOut: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "RequestTimedOut", "(Request timed out)", I, J);
                    }
                    if (J.address.form) {
                        delete J.address.form.processing;
                    }
                    if (J.invoke) {
                        J.address.accept(J, false);
                    }
                    s.enableFields(J.address);
                    s.hideLoader(J.address);
                },
                AddressWasValid: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddressWasValid", "(Response indicates input address was valid)", I, J);
                    }
                    var L = J.address;
                    var K = J.response;
                    J.response.chosen = K.raw[0];
                    L.replaceWith(K.raw[0], true, I);
                    L.accept(J);
                },
                AddressWasAmbiguous: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddressWasAmbiguous", "(Response indiciates input address was ambiguous)", I, J);
                    }
                    s.showAmbiguous(J);
                },
                AddressWasInvalid: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddressWasInvalid", "(Response indicates input address was invalid)", I, J);
                    }
                    s.showInvalid(J);
                },
                CountryWasInvalid: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "CountryWasInvalid", "(Pre-verification check indicates that the country was invalid)", I, J);
                    }
                    s.hideLoader(J.address);
                    s.showInvalidCountry(J);
                },
                AddressWasMissingSecondary: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddressWasMissingSecondary", "(Response indicates input address was missing secondary", I, J);
                    }
                    s.showMissingSecondary(J);
                },
                AddressWasMissingInput: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddressWasMissingInput", "(Pre-verification check indicates that there was not enough input)", I, J);
                    }
                    s.showMissingInput(J);
                },
                AddedSecondary: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddedSecondary", "(User entered a secondary number to attempt revalidation)", I, J);
                    }
                    J.address.verify(J.invoke, J.invokeFn);
                },
                OriginalInputSelected: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "OriginalInputSelected", "(User chose to use original input)", I, J);
                    }
                    J.address.accept(J, false);
                },
                UsedSuggestedAddress: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "UsedSuggestedAddress", "(User chose to a suggested address)", I, J);
                    }
                    J.response.chosen = J.chosenCandidate;
                    J.address.replaceWith(J.chosenCandidate, true, I);
                    J.address.accept(J);
                },
                InvalidAddressRejected: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "InvalidAddressRejected", "(User chose to correct an invalid address)", I, J);
                    }
                    if (J.address.form) {
                        delete J.address.form.processing;
                    }
                    r("Completed", J);
                },
                AddressAccepted: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "AddressAccepted", "(Address marked accepted)", I, J);
                    }
                    if (!J) {
                        J = {};
                    }
                    if (J.address && J.address.form) {
                        delete J.address.form.processing;
                    }
                    if (J.invoke && J.invokeFn) {
                        o(J.invoke, J.invokeFn);
                    }
                    r("Completed", J);
                },
                Completed: function (I, J) {
                    if (A.debug) {
                        console.log("EVENT:", "Completed", "(All done)", I, J);
                    }
                    if (J.address) {
                        s.enableFields(J.address);
                        if (J.address.form) {
                            delete J.address.form.processing;
                        }
                    }
                }
            },
            on: function (J, I) {
                if (!this.events[J] || typeof I !== "function") {
                    return false;
                }
                var K = this.events[J];
                this.events[J] = function (L, M) {
                    I(L, M, K);
                };
            },
            mapFields: function (J) {
                var I = function (K) {
                    if (typeof K === "object") {
                        return s.mapFields(K);
                    }
                    else {
                        if (!K && typeof A.addresses === "object") {
                            return s.mapFields(A.addresses);
                        }
                        else {
                            return false;
                        }
                    }
                };
                if (g.isReady) {
                    I(J);
                }
                else {
                    g(function () {
                        I(J);
                    });
                }
            },
            makeAddress: function (I) {
                if (typeof I !== "object") {
                    return z.getMappedAddressByID(I) || new w({
                        freeform: I
                    });
                }
                else {
                    return new w(I);
                }
            },
            verify: function (I, K) {
                var J = z.makeAddress(I);
                r("VerificationInvoked", {
                    address: J,
                    verifyAccepted: true,
                    invoke: K
                });
            },
            getMappedAddresses: function () {
                var K = [];
                for (var J = 0; J < d.length; J++) {
                    for (var I = 0; I < d[J].addresses.length; I++) {
                        K.push(d[J].addresses[I]);
                    }
                }
                return K;
            },
            getMappedAddressByID: function (I) {
                for (var K = 0; K < d.length; K++) {
                    for (var J = 0; J < d[K].addresses.length; J++) {
                        if (d[K].addresses[J].id() == I) {
                            return d[K].addresses[J];
                        }
                    }
                }
            },
            setKey: function (I) {
                A.key = I;
            },
            setCityFilter: function (I) {
                A.cityFilter = I;
            },
            setStateFilter: function (I) {
                A.stateFilter = I;
            },
            setCityStatePreference: function (I) {
                A.cityStatePreference = I;
            },
            activate: function (I) {
                var J = z.getMappedAddressByID(I);
                if (J) {
                    J.active = true;
                    s.showSmartyUI(I);
                }
            },
            deactivate: function (I) {
                if (!I) {
                    return s.clean();
                }
                var J = z.getMappedAddressByID(I);
                if (J) {
                    J.active = false;
                    J.verifyCount = 0;
                    J.unaccept();
                    s.hideSmartyUI(I);
                }
            },
            autoVerify: function (K) {
                if (typeof K === "undefined") {
                    return A.autoVerify;
                }
                else {
                    if (K === false) {
                        A.autoVerify = false;
                    }
                    else {
                        if (K === true) {
                            A.autoVerify = true;
                        }
                    }
                }
                for (var J = 0; J < d.length; J++) {
                    for (var I = 0; I < d[J].addresses.length; I++) {
                        d[J].addresses[I].verifyCount = 0;
                    }
                }
            },
            version: e
        };
        for (var H in z.events) {
            if (z.events.hasOwnProperty(H)) {
                g(t).off(H, a);
                x(H);
            }
        }
        if (A.target.indexOf("US") >= 0 || A.target.indexOf("INTERNATIONAL") >= 0) {
            z.mapFields();
        }
        else {
            if (A.debug) {
                console.log('Proper target not set in configuration. Please use "US" or "INTERNATIONAL".');
            }
        }
        return z;
    };
    function m() {
        var R;
        var J = "smartyForm";
        var T;
        var Q = 0;
        var M = [];
        var G = 24, L = 8;
        var V = "<style>.smarty-dots { display: none; position: absolute; z-index: 999; width: " + G + "px; height: " + L + 'px; background-image: url("data:image/gif;base64,R0lGODlhGAAIAOMAALSytOTi5MTCxPTy9Ly6vPz6/Ozq7MzKzLS2tOTm5PT29Ly+vPz+/MzOzP///wAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQJBgAOACwAAAAAGAAIAAAEUtA5NZi8jNrr2FBScQAAYVyKQC6gZBDkUTRkXUhLDSwhojc+XcAx0JEGjoRxCRgWjcjAkqZr5WoIiSJIaohIiATqimglg4KWwrDBDNiczgDpiAAAIfkECQYAFwAsAAAAABgACACEVFZUtLK05OLkxMbE9PL0jI6MvL68bG5s7Ors1NbU/Pr8ZGJkvLq8zM7MXFpctLa05ObkzMrM9Pb0nJqcxMLE7O7s/P78////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABWDgZVWQcp2nJREWmhLSKRWOcySoRAWBEZ8IBi+imAAcxwXhZODxDCfFwxloLI6A7OBCoPKWEG/giqxRuOLKRSA2lpVM6kM2dTZmyBuK0Aw8fhcQdQMxIwImLiMSLYkVPyEAIfkECQYAFwAsAAAAABgACACEBAIEpKak1NbU7O7svL68VFZU/Pr8JCIktLK05OLkzMrMDA4M9Pb0vLq87Ors9PL0xMLEZGZk/P78tLa05ObkzM7MFBIU////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABWLgJVGCcZ2n9DASmq7nUwDAQaAPhCAEgzqNncIQodEWgxNht7tdDBMmorIw0gKXh3T3uCSYgV3VitUiwrskZTspGpFKsJMRRVdkNBuKseT5Tg4TUQo+BgkCfygSDCwuIgN/IQAh+QQJBgAXACwAAAAAGAAIAIRUVlS0srTk4uR8enz08vTExsRsbmzs6uyMjoz8+vzU1tRkYmS8urzMzsxcWly0trTk5uR8fnz09vTMyszs7uycmpz8/vz///8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFYOBlUVBynad1QBaaEtIpIY5jKOgxAM5w5IxAYJKo8HgLwmnnAAAGsodQ2FgcnYUL5Nh0QLTTqbXryB6cXcBPEBYaybEL0wm9SNqFWfOWY0Z+JxBSAXkiFAImLiolLoZxIQAh+QQJBgAQACwAAAAAGAAIAIQEAgS0srTc2tz08vTMyszk5uT8+vw0MjS8ury0trTk4uT09vTMzszs6uz8/vw0NjT///8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFWiAELYMjno4gmCfkDItoEEGANKfwAMAjnA1EjWBg1I4G14HHO5gMiWOAEZUqIAIm86eQeo/XrBbA/RqlMceS6RxVa4xZLVHI7QCHn6hQRbAWDSwoKoIiLzEQIQAh+QQJBgAXACwAAAAAGAAIAIRUVlS0srTk4uR8enz08vTExsRsbmzs6uyMjoz8+vzU1tRkYmS8urzMzsxcWly0trTk5uR8fnz09vTMyszs7uycmpz8/vz///8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFY+B1SYQlntYBmeeVQJSZTEHAHCcUOUCEiwqDw4GQNGrIhGgA4DkGIsIC0ARUHsia4AKpOiGXghewyGq5YwCu4Gw6jlnJ0gu9SKvWRKH2AIt0TQN+F0FNRSISMS0XKSuLCQKKIQAh+QQJBgAXACwAAAAAGAAIAIQEAgSkpqTU1tTs7uy8vrxUVlT8+vwkIiS0srTk4uTMyswMDgz09vS8urzs6uz08vTEwsRkZmT8/vy0trTk5uTMzswUEhT///8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFZOB1MY8knhJpnpchUKahIEjjnAxEE8xJHABA4VGhGQ0ighFBEA0swWBkYgxMEpfHkva4BKLBxRaBHdACCHT3C14U0VbkRWlsXgYLcERGJQxOD3Q8PkBCfyMDKygMDIoiDAIJJiEAIfkECQYAFwAsAAAAABgACACEVFZUtLK05OLkxMbE9PL0jI6MvL68bG5s7Ors1NbU/Pr8ZGJkvLq8zM7MXFpctLa05ObkzMrM9Pb0nJqcxMLE7O7s/P78////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABWPgdUmEJZ4WaZ6XAlWmEgUBg5wSRRvSmRwOR0HSoBkVIoMxYBARFgBHdPJYBgSXijVAuAykUsBii5VsK96oelFc9i5K40MkgYInigHtAcHFH28XP1EFXSMwLBcWFRIrJwoCiCEAOw=="); }.smarty-ui { position: absolute; z-index: 999; text-shadow: none; text-align: left; text-decoration: none; }.smarty-popup { padding: 20px 30px; background: #FFFFFF; display: inline-block;box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19); }.smarty-popup-header { text-transform: uppercase; font: bold 10pt/1em sans-serif; color: #CEA737; padding: 12px 0px 0px; text-align: center;}.smarty-popup-ambiguous-header { color: #CEA737; }.smarty-popup-invalid-header { color: #D0021B; }.smarty-popup-missing-input-header { color: #CEA737; }.smarty-popup-typed-address{ font-family: sans-serif; font-size: 10pt; font-style: italic; text-align: center; margin: 15px 0px;}.smarty-popup-secondary-number-form { font-family: sans-serif; margin: 10px auto 20px; padding: 0; border: none; float: none; background: none; width: auto; text-align: center; }#smarty-popup-secondary-number-input-box {width: 200px; font-size: 11pt; margin-bottom: 10px; text-align: center;}#smarty-popup-secondary-number-form-submit-button { line-height: 23px; background: #606060; border: none; color: #fff; border-radius: 3px; padding: 2px 15px; font-size: 11pt; width: 215px; }#smarty-popup-secondary-number-form-submit-button:hover { background: #333; }.smarty-hr { margin-bottom: 15px; }.smarty-choice-list .smarty-choice { background: #FFF; padding: 10px 15px; color: #9B9B9B; margin-bottom: 10px; }.smarty-choice { display: block; font: 300 10pt/1em sans-serif; text-decoration: none; border: 1px solid #D4D4D4; }.smarty-choice-list .smarty-choice:hover { color: #333; background: #F7F7F7; text-decoration: none; border: 1px solid #333 }.smarty-choice-alt { background: inherit; clear: both; }.smarty-choice-alt .smarty-choice-abort, .smarty-choice-override { padding: 8px 10px; color: #FFF; font-size: 10pt; text-decoration: none; background: #606060; border-radius: 3px; border: none; } .smarty-choice-override { float: right } .smarty-choice-abort { float: left }.smarty-choice-alt .smarty-choice:first-child { border-top: 0; }.smarty-choice-abort:hover { background: #333; }.smarty-choice-override:hover { background: #333; }.smarty-tag { position: absolute; display: block; overflow: hidden; font: 15px/1.2em sans-serif; text-decoration: none; width: 20px; height: 18px; border-radius: 25px; transition: all .25s; -moz-transition: all .25s; -webkit-transition: all .25s; -o-transition: all .25s; }.smarty-tag:hover { width: 70px; text-decoration: none; color: #999; }.smarty-tag:hover .smarty-tag-text { color: #000; }.smarty-tag-grayed { border: 1px solid #B4B4B4; color: #999; background: #DDD; box-shadow: inset 0 9px 15px #FFF; }.smarty-tag-green { border: 1px solid #407513; color: #407513; background: #A6D187; box-shadow: inset 0 9px 15px #E3F6D5; }.smarty-tag-grayed:hover { border-color: #333; }.smarty-tag-check { padding-left: 4px; text-decoration: none; }.smarty-tag-text { font-size: 12px; position: absolute; top: 0; left: 16px; width: 50px; text-align: center; }.smarty-autocomplete { border: 1px solid #777; background: white; overflow: hidden; white-space: nowrap; box-shadow: 1px 1px 3px #555; }.smarty-suggestion { display: block; color: #444; text-decoration: none; font-size: 12px; padding: 1px 5px; }.smarty-active-suggestion { background: #EEE; color: #000; border: none; outline: none; }.smarty-no-suggestions { padding: 1px 5px; font-size: 12px; color: #AAA; font-style: italic; } .smarty-debug-input { background: #ffffcc !important; } .smarty-debug-button { color: #4ba341 !important; } </style>';
        this.postMappingOperations = function () {
            if (A.ui) {
                g("head").prepend(V);
                var aa = z.getMappedAddresses();
                for (var ad = 0; ad < aa.length; ad++) {
                    var Y = aa[ad].id();
                    g("body").append('<div class="smarty-ui"><div title="Loading..." class="smarty-dots smarty-addr-' + Y + '"></div></div>');
                    if (A.smartyTag) {
                        var ab = U(aa[ad].corners(true));
                        g("body").append('<div class="smarty-ui" style="top: ' + ab.top + "px; left: " + ab.left + 'px;"><a href="javascript:" class="smarty-tag smarty-tag-grayed smarty-addr-' + Y + '" title="Address not verified. Click to verify." data-addressid="' + Y + '"><span class="smarty-tag-check">&#10003;</span><span class="smarty-tag-text">Verify</span></a></div>');
                    }
                    g(l).on("resize.smarty", {
                        addr: aa[ad]
                    }, function (an) {
                        var aq = an.data.addr;
                        if (A.smartyTag) {
                            var ap = U(aq.corners(true));
                            g(".smarty-tag.smarty-addr-" + aq.id()).parent(".smarty-ui").css("top", ap.top + "px").css("left", ap.left + "px");
                        }
                        var ak = aq.corners();
                        g(".smarty-popup.smarty-addr-" + aq.id()).parent(".smarty-ui").css("top", ak.top + "px").css("left", ak.left + "px");
                        if (A.autocomplete) {
                            var ao = g(".smarty-autocomplete.smarty-addr-" + aq.id()).closest(".smarty-ui");
                            var am = aq.getDomFields();
                            var al = "";
                            if (am.address1) {
                                al = "address1";
                            }
                            else {
                                if (am.freeform) {
                                    al = "freeform";
                                }
                            }
                            if (al !== "") {
                                ao.css({
                                    left: g(am[al]).offset().left + "px",
                                    top: (g(am[al]).offset().top + g(am[al]).outerHeight(false)) + "px"
                                });
                            }
                        }
                    });
                }
                if (A.smartyTag) {
                    g("body").on("click", ".smarty-tag-grayed", function (al) {
                        var ak = g(this).data("addressid");
                        z.verify(ak);
                    });
                    g("body").on("click", ".smarty-undo", function (al) {
                        var ak = g(this).parent().data("addressid");
                        var am = z.getMappedAddressByID(ak);
                        am.undo(true);
                    });
                }
                if (A.autocomplete && A.key) {
                    for (var ad = 0; ad < d.length; ad++) {
                        var af = d[ad];
                        for (var ac = 0; ac < af.addresses.length; ac++) {
                            var ag = af.addresses[ac];
                            var aj = ag.getDomFields();
                            var ai = "";
                            if (aj.address1) {
                                ai = "address1";
                            }
                            else {
                                if (aj.freeform) {
                                    ai = "freeform";
                                }
                            }
                            if (ai !== "") {
                                var ah = g(aj[ai]);
                                var X = g('<div class="smarty-ui"></div>');
                                var Z = g('<div class="smarty-autocomplete"></div>');
                                Z.addClass("smarty-addr-" + ag.id());
                                X.data("addrID", ag.id());
                                X.append(Z);
                                X.css({
                                    position: "absolute",
                                    left: ah.offset().left + "px",
                                    top: (ah.offset().top + ah.outerHeight(false)) + "px"
                                });
                                X.hide().appendTo("body");
                                X.on("mousedown", ".smarty-suggestion", {
                                    addr: ag,
                                    containerUi: X
                                }, function (al) {
                                    var ak = T.suggestions[g(this).data("suggIndex")];
                                    N(al.data.addr, ak, al.data.containerUi);
                                });
                                X.on("mouseover", ".smarty-suggestion", function () {
                                    g(".smarty-active-suggestion").removeClass("smarty-active-suggestion");
                                    g(this).addClass("smarty-active-suggestion");
                                });
                                X.on("mouseleave", ".smarty-active-suggestion", function () {
                                    g(this).removeClass("smarty-active-suggestion");
                                });
                                ah.keydown({
                                    containerUi: X,
                                    addr: ag
                                }, function (an) {
                                    var al = g(".smarty-autocomplete", an.data.containerUi);
                                    var ak = g(".smarty-active-suggestion:visible", al).first();
                                    var am = false;
                                    if (an.keyCode == 9) {
                                        if (ak.length > 0) {
                                            var ao = an.data.addr.getDomFields();
                                            if (ao.zipcode) {
                                                g(ao.zipcode).focus();
                                            }
                                            else {
                                                g(ao[ai]).blur();
                                            }
                                            N(an.data.addr, T.suggestions[ak.data("suggIndex")], an.data.containerUi);
                                            return ag.isFreeform() ? true : v(an);
                                        }
                                        else {
                                            s.hideAutocomplete(an.data.addr.id());
                                        }
                                    }
                                    else {
                                        if (an.keyCode == 40) {
                                            if (!ak.hasClass("smarty-suggestion")) {
                                                ak = g(".smarty-suggestion", al).first().mouseover();
                                                am = true;
                                            }
                                            if (!am) {
                                                if (ak.next(".smarty-addr-" + an.data.addr.id() + " .smarty-suggestion").length > 0) {
                                                    ak.next(".smarty-suggestion").mouseover();
                                                }
                                                else {
                                                    ak.removeClass("smarty-active-suggestion");
                                                }
                                            }
                                            H(this);
                                        }
                                        else {
                                            if (an.keyCode == 38) {
                                                if (!ak.hasClass("smarty-suggestion")) {
                                                    ak = g(".smarty-suggestion", al).last().mouseover();
                                                    am = true;
                                                }
                                                if (!am) {
                                                    if (ak.prev(".smarty-addr-" + an.data.addr.id() + " .smarty-suggestion").length > 0) {
                                                        ak.prev(".smarty-suggestion").mouseover();
                                                    }
                                                    else {
                                                        ak.removeClass("smarty-active-suggestion");
                                                    }
                                                }
                                                H(this);
                                            }
                                        }
                                    }
                                });
                                ah.keyup({
                                    form: af,
                                    addr: ag,
                                    streetField: ah,
                                    containerUi: X
                                }, S);
                            }
                        }
                        g(t).keyup(function (ak) {
                            if (ak.keyCode == 27) {
                                g(".smarty-autocomplete").closest(".smarty-ui").hide();
                            }
                        });
                    }
                    setTimeout(function () {
                        g(l).trigger("resize.smarty");
                    }, 500);
                    setTimeout(function () {
                        g(l).trigger("resize.smarty");
                    }, 1500);
                }
            }
            if (A.submitVerify) {
                for (var ad = 0; ad < d.length; ad++) {
                    var af = d[ad];
                    R = function (al) {
                        if ((al.data.form && al.data.form.processing) || g(".smarty-active-suggestion:visible").length > 0) {
                            return v(al);
                        }
                        if (!al.data.form.allActiveAddressesAccepted()) {
                            var ak = al.data.form.activeAddressesNotAccepted();
                            if (ak.length > 0) {
                                r("VerificationInvoked", {
                                    address: ak[0],
                                    invoke: al.data.invoke,
                                    invokeFn: al.data.invokeFn
                                });
                            }
                            return v(al);
                        }
                    };
                    var W = function (ap, am) {
                        if (!ap || !am) {
                            return;
                        }
                        var ao = [], ak = g._data(ap, "events");
                        if (ak && ak[am] && ak[am].length > 0) {
                            ao = g.extend(true, [], ak[am]);
                        }
                        g(ap).off(am);
                        g(ap)[am]({
                            form: af,
                            invoke: ap,
                            invokeFn: am
                        }, R);
                        if (typeof ap["on" + am] === "function") {
                            var al = ap["on" + am];
                            ap["on" + am] = null;
                            g(ap)[am](al);
                        }
                        for (var an = 0; an < ao.length; an++) {
                            g(ap)[am](ao[an].data, ao[an].handler);
                        }
                    };
                    var ae = g(A.submitSelector, af.dom);
                    if (A.debug) {
                        for (var ac = 0; ac < ae.length; ac++) {
                            g(ae[ac]).addClass("smarty-debug-button");
                        }
                    }
                    ae.each(function (ak) {
                        W(this, "click");
                    });
                }
            }
            r("MapInitialized");
        };
        function S(Y) {
            var ab = Y.data.addr;
            var Z = Y.data.streetField;
            var X = g.trim(Y.data.streetField.val());
            var aa = Y.data.containerUi;
            var W = g(".smarty-autocomplete", aa);
            if (!X) {
                ab.lastStreetInput = X;
                W.empty();
                s.hideAutocomplete(ab.id());
            }
            if (Y.keyCode == 13) {
                if (g(".smarty-active-suggestion:visible").length > 0) {
                    N(ab, T.suggestions[g(".smarty-active-suggestion:visible").first().data("suggIndex")], aa);
                }
                s.hideAutocomplete(ab.id());
                Z.blur();
                return v(Y);
            }
            if (Y.keyCode == 40) {
                H(Z[0]);
                return;
            }
            if (Y.keyCode == 38) {
                H(Z[0]);
                return;
            }
            if (!X || X == ab.lastStreetInput || !ab.isDomestic()) {
                return;
            }
            ab.lastStreetInput = X;
            if (ab.isDomestic() && A.target.indexOf("US") >= 0) {
                r("AutocompleteInvoked", {
                    containerUi: aa,
                    suggContainer: W,
                    streetField: Z,
                    input: X,
                    addr: ab
                });
            }
        }
        this.requestAutocomplete = function (W, Y) {
            if (Y.input && Y.addr.isDomestic() && T) {
                Y.containerUi.show();
            }
            var Z = {
                callback: function (aa, ac) {
                    var af = new RegExp("^\\w+\\s\\w+|^[A-Za-z]+$|^[A-Za-z]+\\s\\w*");
                    var ah = af.test(Y.input);
                    T = ac;
                    Y.suggContainer.empty();
                    if (!ac.suggestions || ac.suggestions.length == 0) {
                        Y.suggContainer.html('<div class="smarty-no-suggestions">' + A.noSuggestionsMessage + "</div>");
                        return;
                    }
                    if (A.waitForStreet && ah == false) {
                        var ae = "";
                        if (A.stateFilter || A.cityFilter || A.geolocate || A.cityStatePreference) {
                            ae = "filtered";
                        }
                        else {
                            ae = "address";
                        }
                        Y.suggContainer.html('<div class="smarty-no-suggestions">Type more for ' + ae + " suggestions</div>");
                    }
                    else {
                        for (var ab = 0; ab < ac.suggestions.length; ab++) {
                            var ag = ac.suggestions[ab].text.replace(/<|>/g, "");
                            ag = ag.replace(new RegExp("(" + Y.input.replace(/[#-.]|[[-^]|[?|{}]/g, "\\$&") + ")", "ig"), "<b>$1</b>");
                            var ad = g('<a href="javascript:" class="smarty-suggestion">' + ag + "</a>");
                            ad.data("suggIndex", ab);
                            Y.suggContainer.append(ad);
                        }
                    }
                    Y.suggContainer.css({
                        width: Math.max(Y.streetField.outerWidth(false), 250) + "px"
                    });
                    Y.containerUi.show();
                    M.splice(0, aa);
                },
                number: Q++
            };
            M[Z.number] = Z;
            var X = {
                url: "https://us-autocomplete.api.smartystreets.com/suggest",
                traditional: true,
                dataType: "json",
                data: {
                    "auth-id": A.key,
                    "auth-token": A.token,
                    prefix: Y.input,
                    city_filter: A.cityFilter,
                    state_filter: A.stateFilter,
                    prefer: A.cityStatePreference,
                    suggestions: A.autocomplete,
                    geolocate: A.geolocate,
                    geolocate_precision: A.geolocatePrecision,
                    prefer_ratio: A.preferRatio,
                    agent: ["smartystreets (plugin:website@" + z.version + ")", A.agent]
                }
            };
            g.ajax(g.extend({}, A.ajaxSettings, X)).done(function (aa) {
                r("AutocompleteReceived", g.extend(Y, {
                    json: aa,
                    autocplrequest: Z
                }));
            });
        };
        this.showAutocomplete = function (W, X) {
            if (M[X.autocplrequest.number]) {
                M[X.autocplrequest.number].callback(X.autocplrequest.number, X.json);
            }
        };
        function N(aa, X, Z) {
            aa.usedAutocomplete = false;
            var W = aa.getDomFields();
            s.hideAutocomplete(aa.id());
            if (aa.isFreeform()) {
                g(W.freeform).val(X.text).change();
                aa.usedAutocomplete = true;
            }
            else {
                if (W.postal_code) {
                    g(W.postal_code).val("").change();
                }
                if (W.address1) {
                    g(W.address1).val(X.street_line).change();
                }
                if (W.administrative_area) {
                    if (W.administrative_area.options) {
                        for (var Y = 0; Y < W.administrative_area.options.length; Y++) {
                            if (W.administrative_area.options[Y].text.toUpperCase() === X.state || n[W.administrative_area.options[Y].text.toUpperCase()] === X.state) {
                                g(W.administrative_area)[0].selectedIndex = Y;
                                g(W.administrative_area).change();
                                break;
                            }
                        }
                    }
                    else {
                        g(W.administrative_area).val(X.state).change();
                    }
                }
                if (W.locality) {
                    g(W.locality).val("").change();
                    aa.usedAutocomplete = true;
                    g(W.locality).val(X.city).change();
                }
            }
            if (W.country && !W.country.options) {
                g(W.country).val("USA").change();
            }
            r("AutocompleteUsed", {
                address: aa,
                suggestion: X
            });
        }
        function U(W) {
            return {
                top: W.top + W.height / 2 - 10,
                left: W.right - 6
            };
        }
        function P(aa, ae, ab) {
            var W = h(aa.name);
            var X = h(aa.id);
            var ac = X.replace(/[\[|\]|\(|\)|\:|\'|\"|\=|\||\#|\.|\!|\||\@|\^|\&|\*]/g, "\\\\$&");
            var ag = h(aa.placeholder);
            var af = h(aa.title);
            for (var Z = 0; Z < ae.length; Z++) {
                if (W.indexOf(ae[Z]) > -1 || X.indexOf(ae[Z]) > -1) {
                    return true;
                }
            }
            if (!("labels" in aa)) {
                var ad = g('label[for="' + ac + '"]')[0] || g(aa).parents("label")[0];
                aa.labels = !ad ? [] : [ad];
            }
            for (var Z = 0; Z < aa.labels.length; Z++) {
                for (var Y = 0; Y < ab.length; Y++) {
                    if (g(aa.labels[Z]).text().toLowerCase().indexOf(ab[Y]) > -1) {
                        return true;
                    }
                }
            }
            for (var Z = 0; Z < ab.length; Z++) {
                if (ag.indexOf(ab[Z]) > -1 || af.indexOf(ab[Z]) > -1) {
                    return true;
                }
            }
            return false;
        }
        function O(W, X) {
            g(t).off("keyup");
            g(W).slideUp(p.speed, function () {
                g(this).parent(".smarty-ui").remove();
            });
            r("Completed", X.data);
        }
        function I(X) {
            if (Array.isArray(X) || typeof X == "object") {
                for (var W in X) {
                    if (X.hasOwnProperty(W)) {
                        g("body").off("click", X[W]);
                    }
                }
            }
            else {
                if (typeof X === "string") {
                    g("body").off("click", X);
                }
                else {
                    alert("ERROR: Not an array, string, or object passed in to turn off all clicks");
                }
            }
        }
        function H(X) {
            if (typeof X.selectionStart == "number") {
                X.selectionStart = X.selectionEnd = X.value.length;
            }
            else {
                if (typeof X.createTextRange != "undefined") {
                    X.focus();
                    var W = X.createTextRange();
                    W.collapse(false);
                    W.select();
                }
            }
        }
        this.hideAutocomplete = function (W) {
            g(".smarty-autocomplete.smarty-addr-" + W).closest(".smarty-ui").hide();
        };
        this.showSmartyUI = function (W) {
            var X = g(".deactivated.smarty-addr-" + W);
            X.push(X[0].parentElement);
            X.removeClass("deactivated");
            X.addClass("activated");
            X.show();
        };
        this.hideSmartyUI = function (W) {
            var X = g(".smarty-addr-" + W + ":visible");
            var Y = g(".smarty-autocomplete.smarty-addr-" + W);
            X.addClass("deactivated");
            X.parent().addClass("deactivated");
            Y.addClass("deactivated");
            X.hide();
            X.parent().hide();
            Y.hide();
        };
        this.clean = function () {
            if (d.length == 0) {
                return;
            }
            if (A.debug) {
                console.log("Cleaning up old form map data and bindings...");
            }
            for (var Y = 0; Y < d.length; Y++) {
                g(d[Y].dom).data(J, "");
                for (var X = 0; X < d[Y].addresses.length; X++) {
                    var aa = d[Y].addresses[X].getDomFields();
                    for (var ab in aa) {
                        if (aa.hasOwnProperty(ab)) {
                            if (A.debug) {
                                g(aa[ab]).removeClass("smarty-debug-input").attr("placeholder", "");
                                var Z = g(A.submitSelector);
                                for (var W = 0; W < Z.length; W++) {
                                    g(Z[W]).removeClass("smarty-debug-button");
                                }
                            }
                            g(aa[ab]).off("change", i);
                        }
                    }
                    if (aa.address1) {
                        g(aa.address1).off("keyup").off("keydown").off("blur");
                    }
                    else {
                        if (aa.freeform) {
                            g(aa.freeform).off("keyup").off("keydown").off("blur");
                        }
                    }
                }
                g.each(d, function (ac) {
                    g(this.dom).off("submit", R);
                });
                g(A.submitSelector, d[Y].dom).each(function (ac) {
                    g(this).off("click", R);
                });
            }
            g(".smarty-ui").off("click", ".smarty-suggestion").off("mouseover", ".smarty-suggestion").off("mouseleave", ".smarty-suggestion").remove();
            if (A.smartyTag) {
                g("body").off("click", ".smarty-undo");
                g("body").off("click", ".smarty-tag-grayed");
            }
            g(l).off("resize.smarty");
            g(t).off("keyup");
            d = [];
            E = 0;
            if (A.debug) {
                console.log("Done cleaning up; ready for new mapping.");
            }
        };
        function F(W) {
            if (A.autocomplete > 0) {
                Object.keys(W).map(function (X) {
                    var Y = g(W[X]);
                    if (Y.length > 0) {
                        Y.attr("autocomplete", "smartystreets");
                    }
                });
            }
        }
        function K(X) {
            if (X.getElementsByTagName("option").length > 0) {
                if (y(q, X.getElementsByTagName("option")[0].text.toUpperCase()) || y(u, X.getElementsByTagName("option")[0].text.toUpperCase())) {
                    var W = t.createElement("OPTION");
                    W.innerText = "Pick a state";
                    W.selected = true;
                    g(X.getElementsByTagName("select")[0]).prepend(W);
                    g(X).change();
                }
            }
        }
        this.mapFields = function (W) {
            if (A.debug) {
                console.log("Manually mapping fields given this data:", W);
            }
            this.clean();
            var ad = [];
            W = W instanceof Array ? W : [W];
            for (var ae in W) {
                if (W.hasOwnProperty(ae)) {
                    var af = W[ae];
                    if (!af.country && A.target.indexOf("US") < 0) {
                        continue;
                    }
                    var ab = "";
                    for (var ac in af) {
                        if (af.hasOwnProperty(ac)) {
                            if (ac == "match") {
                                ab = af[ac];
                                delete af[ac];
                            }
                            if (ac != "id") {
                                if (!y(f, ac)) {
                                    if (A.debug) {
                                        console.log("NOTICE: Field named " + ac + " is not allowed. Skipping...");
                                    }
                                    delete af[ac];
                                    continue;
                                }
                                var Y = g(af[ac]);
                                if (Y.length == 0) {
                                    if (A.debug) {
                                        console.log("NOTICE: No matches found for selector " + af[ac] + ". Skipping...");
                                    }
                                    delete af[ac];
                                }
                                else {
                                    if (Y.parents("form").length == 0) {
                                        if (A.debug) {
                                            console.log('NOTICE: Element with selector "' + af[ac] + '" is not inside a <form> tag. Skipping...');
                                        }
                                        delete af[ac];
                                    }
                                    else {
                                        af[ac] = Y[0];
                                    }
                                }
                            }
                        }
                    }
                    if (A.target.indexOf("INTERNATIONAL") >= 0) {
                        if (!((af.country && af.freeform) || (af.country && af.address1 && af.postal_code) || (af.country && af.address1 && af.locality && af.administrative_area))) {
                            if (A.debug) {
                                console.log("NOTICE: Address map (index " + ae + ") was not mapped to a complete street address. Skipping...");
                            }
                            continue;
                        }
                    }
                    else {
                        if (!((af.freeform) || (af.address1 && af.postal_code) || (af.address1 && af.locality && af.administrative_area))) {
                            if (A.debug) {
                                console.log("NOTICE: Address map (index " + ae + ") was not mapped to a complete street address. Skipping...");
                            }
                            continue;
                        }
                    }
                    var aa = g(af.address1).parents("form")[0];
                    if (!aa) {
                        aa = g(af.freeform).parents("form")[0];
                    }
                    var X = new j(aa);
                    if (!g(aa).data(J)) {
                        g(aa).data(J, 1);
                        F(af);
                        K(X.dom);
                        ad.push(X);
                    }
                    else {
                        for (var Z = 0; Z < ad.length; Z++) {
                            if (ad[Z].dom == aa) {
                                X = ad[Z];
                                break;
                            }
                        }
                    }
                    E++;
                    X.addresses.push(new w(af, X, af.id, ab));
                    if (A.debug) {
                        console.log("Finished mapping address with ID: " + X.addresses[X.addresses.length - 1].id());
                    }
                }
            }
            d = ad;
            r("FieldsMapped");
        };
        this.disableFields = function (X) {
            if (!A.ui) {
                return;
            }
            var W = X.getDomFields();
            for (var Z in W) {
                if (W.hasOwnProperty(Z)) {
                    g(W[Z]).prop ? g(W[Z]).prop("disabled", true) : g(W[Z]).attr("disabled", "disabled");
                }
            }
            if (X.form && X.form.dom) {
                var Y = g(A.submitSelector, X.form.dom);
                Y.prop ? Y.prop("disabled", true) : Y.attr("disabled", "disabled");
            }
        };
        this.enableFields = function (X) {
            if (!A.ui) {
                return;
            }
            var W = X.getDomFields();
            for (var Z in W) {
                if (W.hasOwnProperty(Z)) {
                    g(W[Z]).prop ? g(W[Z]).prop("disabled", false) : g(W[Z]).removeAttr("disabled");
                }
            }
            if (X.form && X.form.dom) {
                var Y = g(A.submitSelector, X.form.dom);
                Y.prop ? Y.prop("disabled", false) : Y.removeAttr("disabled");
            }
        };
        this.showLoader = function (Y) {
            if (!A.ui || !Y.hasDomFields()) {
                return;
            }
            var W = Y.corners(true);
            var X = g(".smarty-dots.smarty-addr-" + Y.id()).parent();
            X.css("top", (W.top + W.height / 2 - L / 2) + "px").css("left", (W.right - G - 10) + "px");
            g(".smarty-dots", X).show();
        };
        this.hideLoader = function (W) {
            if (A.ui) {
                g(".smarty-dots.smarty-addr-" + W.id()).hide();
            }
        };
        this.markAsValid = function (X) {
            if (!A.ui || !X || !A.smartyTag) {
                return;
            }
            var W = g(".smarty-tag.smarty-tag-grayed.smarty-addr-" + X.id());
            W.removeClass("smarty-tag-grayed").addClass("smarty-tag-green").attr("title", "Address verified! Click to undo.");
            g(".smarty-tag-text", W).text("Verified").hover(function () {
                g(this).text("Undo");
            }, function () {
                g(this).text("Verified");
            }).addClass("smarty-undo");
        };
        this.unmarkAsValid = function (Y) {
            var X = ".smarty-tag.smarty-addr-" + Y.id();
            if (!A.ui || !Y || !A.smartyTag || g(X).length == 0) {
                return;
            }
            var W = g(".smarty-tag.smarty-tag-green.smarty-addr-" + Y.id());
            W.removeClass("smarty-tag-green").addClass("smarty-tag-grayed").attr("title", "Address not verified. Click to verify.");
            g(".smarty-tag-text", W).text("Verify").off("mouseenter mouseleave").removeClass("smarty-undo");
        };
        this.showAmbiguous = function (aa) {
            if (!A.ui || !aa.address.hasDomFields()) {
                return;
            }
            var ae = aa.address;
            var Y = aa.response;
            var ad = ae.corners();
            ad.width = 294;
            var ac = '<div class="smarty-ui" style="top: ' + ad.top + "px; left: " + ad.left + 'px;"><div class="smarty-popup smarty-addr-' + ae.id() + '" style="width: ' + ad.width + 'px;"><div class="smarty-popup-header smarty-popup-ambiguous-header">' + A.ambiguousMessage + '</div><div class="smarty-popup-typed-address">' + ae.toString() + '</div><div class="smarty-choice-list">';
            if (ae.isDomestic()) {
                for (var ab = 0; ab < Y.raw.length; ab++) {
                    var ag = Y.raw[ab].delivery_line_1, Z = Y.raw[ab].components.city_name, ah = Y.raw[ab].components.state_abbreviation, X = Y.raw[ab].components.zipcode + "-" + Y.raw[ab].components.plus4_code;
                    ac += '<a href="javascript:" class="smarty-choice" data-index="' + ab + '">' + ag + "<br>" + Z + ", " + ah + " " + X + "</a>";
                }
            }
            else {
                var W = A.candidates;
                if (Y.raw.length < W) {
                    W = Y.raw.length;
                }
                for (var ab = 0; ab < W; ab++) {
                    var af = "";
                    if (Y.raw[ab].address1) {
                        af += Y.raw[ab].address1;
                    }
                    if (Y.raw[ab].address2) {
                        af = af + "<br>" + Y.raw[ab].address2;
                    }
                    if (Y.raw[ab].address3) {
                        af = af + "<br>" + Y.raw[ab].address3;
                    }
                    if (Y.raw[ab].address4) {
                        af = af + "<br>" + Y.raw[ab].address4;
                    }
                    if (Y.raw[ab].address5) {
                        af = af + "<br>" + Y.raw[ab].address5;
                    }
                    if (Y.raw[ab].address6) {
                        af = af + "<br>" + Y.raw[ab].address6;
                    }
                    ac += '<a href="javascript:" class="smarty-choice" data-index="' + ab + '">' + af + "</a>";
                }
            }
            ac += '</div><div class="smarty-choice-alt">';
            ac += '<a href="javascript:" class="smarty-choice smarty-choice-abort smarty-abort">' + A.changeMessage + "</a>";
            if (!A.enforceVerification) {
                ac += '<a href="javascript:" class="smarty-choice smarty-choice-override">' + A.certifyMessage + "</a>";
            }
            ac += "</div></div></div>";
            g(ac).hide().appendTo("body").show(p.speed);
            if (g(t).scrollTop() > ad.top - 100 || g(t).scrollTop() < ad.top - g(l).height() + 100) {
                g("html, body").stop().animate({
                    scrollTop: g(".smarty-popup.smarty-addr-" + ae.id()).offset().top - 100
                }, 500);
            }
            aa.selectors = {
                goodAddr: ".smarty-popup.smarty-addr-" + ae.id() + " .smarty-choice-list .smarty-choice",
                useOriginal: ".smarty-popup.smarty-addr-" + ae.id() + " .smarty-choice-override",
                abort: ".smarty-popup.smarty-addr-" + ae.id() + " .smarty-abort"
            };
            g("body").on("click", aa.selectors.goodAddr, aa, function (ai) {
                g(".smarty-popup.smarty-addr-" + ae.id()).slideUp(p.speed, function () {
                    g(this).parent(".smarty-ui").remove();
                    g(this).remove();
                });
                I(ai.data.selectors);
                delete ai.data.selectors;
                r("UsedSuggestedAddress", {
                    address: ai.data.address,
                    response: ai.data.response,
                    invoke: ai.data.invoke,
                    invokeFn: ai.data.invokeFn,
                    chosenCandidate: Y.raw[g(this).data("index")]
                });
            });
            g("body").on("click", aa.selectors.useOriginal, aa, function (ai) {
                g(this).parents(".smarty-popup").slideUp(p.speed, function () {
                    g(this).parent(".smarty-ui").remove();
                    g(this).remove();
                });
                I(ai.data.selectors);
                delete ai.data.selectors;
                r("OriginalInputSelected", ai.data);
            });
            g(t).keyup(aa, function (ai) {
                if (ai.keyCode == 27) {
                    I(ai.data.selectors);
                    delete ai.data.selectors;
                    O(g(".smarty-popup.smarty-addr-" + ai.data.address.id()), ai);
                    v(ai);
                }
            });
            g("body").on("click", aa.selectors.abort, aa, function (ai) {
                I(ai.data.selectors);
                delete ai.data.selectors;
                O(g(this).parents(".smarty-popup"), ai);
            });
        };
        this.showInvalid = function (Y) {
            if (!A.ui || !Y.address.hasDomFields()) {
                return;
            }
            var Z = Y.address;
            var W = Z.corners();
            W.width = 300;
            var X = '<div class="smarty-ui" style="top: ' + W.top + "px; left: " + W.left + 'px;"><div class="smarty-popup smarty-addr-' + Z.id() + '" style="width: ' + W.width + 'px;"><div class="smarty-popup-header smarty-popup-invalid-header">' + A.invalidMessage + '</div><div class="smarty-popup-typed-address">' + htmlDecode(Z.toString()) + '</div><div class="smarty-choice-alt"><a href="javascript:" class="smarty-choice smarty-choice-abort smarty-abort">' + A.changeMessage + "</a>";
            if (!A.enforceVerification) {
                X += '<a href="javascript:" class="smarty-choice smarty-choice-override">' + A.certifyMessage + "</a>";
            }
            X += "</div></div>";
            g(X).hide().appendTo("body").show(p.speed);
            Y.selectors = {
                useOriginal: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-choice-override ",
                abort: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-abort"
            };
            if (g(t).scrollTop() > W.top - 100 || g(t).scrollTop() < W.top - g(l).height() + 100) {
                g("html, body").stop().animate({
                    scrollTop: g(".smarty-popup.smarty-addr-" + Z.id()).offset().top - 100
                }, 500);
            }
            I(Y.selectors.abort);
            g("body").on("click", Y.selectors.abort, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("InvalidAddressRejected", aa.data);
            });
            I(Y.selectors.useOriginal);
            g("body").on("click", Y.selectors.useOriginal, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("OriginalInputSelected", aa.data);
            });
            g(t).keyup(Y, function (aa) {
                if (aa.keyCode == 27) {
                    I(aa.data.selectors);
                    g(Y.selectors.abort).click();
                    O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                }
            });
        };
        this.showInvalidCountry = function (Y) {
            if (!A.ui || !Y.address.hasDomFields()) {
                return;
            }
            var Z = Y.address;
            var W = Z.corners();
            W.width = 300;
            var X = '<div class="smarty-ui" style="top: ' + W.top + "px; left: " + W.left + 'px;"><div class="smarty-popup smarty-addr-' + Z.id() + '" style="width: ' + W.width + 'px;"><div class="smarty-popup-header smarty-popup-invalid-header">' + A.invalidCountryMessage + '</div><div class="smarty-popup-typed-address">' + htmlDecode(Z.toString()) + '</div><div class="smarty-choice-alt"><a href="javascript:" class="smarty-choice smarty-choice-abort smarty-abort">' + A.changeMessage + "</a></div>";
            if (!A.enforceVerification) {
                X += '<a href="javascript:" class="smarty-choice smarty-choice-override">' + A.certifyMessage + "</a>";
            }
            X += "</div></div>";
            g(X).hide().appendTo("body").show(p.speed);
            Y.selectors = {
                useOriginal: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-choice-override ",
                abort: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-abort"
            };
            if (g(t).scrollTop() > W.top - 100 || g(t).scrollTop() < W.top - g(l).height() + 100) {
                g("html, body").stop().animate({
                    scrollTop: g(".smarty-popup.smarty-addr-" + Z.id()).offset().top - 100
                }, 500);
            }
            I(Y.selectors.abort);
            g("body").on("click", Y.selectors.abort, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("InvalidAddressRejected", aa.data);
            });
            I(Y.selectors.useOriginal);
            g("body").on("click", Y.selectors.useOriginal, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("OriginalInputSelected", aa.data);
            });
            g(t).keyup(Y, function (aa) {
                if (aa.keyCode == 27) {
                    I(aa.data.selectors);
                    g(Y.selectors.abort).click();
                    O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                }
            });
        };
        this.showMissingSecondary = function (Y) {
            if (!A.ui || !Y.address.hasDomFields()) {
                return;
            }
            var Z = Y.address;
            var W = Z.corners();
            W.width = 300;
            var X = '<div class="smarty-ui" style="top: ' + W.top + "px; left: " + W.left + 'px;"><div class="smarty-popup smarty-addr-' + Z.id() + '" style="width: ' + W.width + 'px;"><div class="smarty-popup-header smarty-popup-missing-secondary-header">' + A.missingSecondaryMessage + '</div><div class="smarty-popup-typed-address">' + Z.toString() + '</div><form class="smarty-popup-secondary-number-form"><input id="smarty-popup-secondary-number-input-box" class="smarty-addr-' + Z.id() + '" type="text" name="secondarynumber" placeholder="Enter number here"><br><input id="smarty-popup-secondary-number-form-submit-button" class="smarty-addr-' + Z.id() + '" type="submit" value="Submit"></form><hr class="smarty-hr"><div class="smarty-choice-alt"><a href="javascript:" class="smarty-choice smarty-choice-abort smarty-abort">' + A.changeMessage + "</a>";
            if (!A.enforceVerification) {
                X += '<a href="javascript:" class="smarty-choice smarty-choice-override">' + A.certifyMessage + "</a>";
            }
            X += "</div></div></div>";
            g(X).hide().appendTo("body").show(p.speed);
            Y.selectors = {
                useOriginal: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-choice-override ",
                abort: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-abort",
                submit: "#smarty-popup-secondary-number-form-submit-button.smarty-addr-" + Z.id()
            };
            if (g(t).scrollTop() > W.top - 100 || g(t).scrollTop() < W.top - g(l).height() + 100) {
                g("html, body").stop().animate({
                    scrollTop: g(".smarty-popup.smarty-addr-" + Z.id()).offset().top - 100
                }, 500);
            }
            I(Y.selectors.abort);
            g("body").on("click", Y.selectors.abort, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("InvalidAddressRejected", aa.data);
            });
            I(Y.selectors.useOriginal);
            g("body").on("click", Y.selectors.useOriginal, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("OriginalInputSelected", aa.data);
            });
            I(Y.selectors.submit);
            g("body").on("click", Y.selectors.submit, Y, function (aa) {
                aa.data.address.secondary = g("#smarty-popup-secondary-number-input-box.smarty-addr-" + aa.data.address.id()).val();
                if (aa.data.address.isFreeform()) {
                    aa.data.address.address1 = aa.data.response.raw[0].delivery_line_1;
                }
                aa.data.address.zipcode = aa.data.response.raw[0].components.zipcode;
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                v(aa);
                r("AddedSecondary", aa.data);
            });
            g(t).keyup(Y, function (aa) {
                if (aa.keyCode == 27) {
                    I(aa.data.selectors);
                    g(Y.selectors.abort).click();
                    O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                }
            });
        };
        this.showMissingInput = function (Y) {
            if (!A.ui || !Y.address.hasDomFields()) {
                return;
            }
            var Z = Y.address;
            var W = Z.corners();
            W.width = 300;
            var X = '<div class="smarty-ui" style="top: ' + W.top + "px; left: " + W.left + 'px;"><div class="smarty-popup smarty-addr-' + Z.id() + '" style="width: ' + W.width + 'px;"><div class="smarty-popup-header smarty-popup-missing-input-header">' + A.missingInputMessage + '</div><div class="smarty-popup-typed-address">' + Z.toString() + '</div><div class="smarty-choice-alt"><a href="javascript:" class="smarty-choice smarty-choice-abort smarty-abort">' + A.changeMessage + "</a>";
            if (!A.enforceVerification) {
                X += '<a href="javascript:" class="smarty-choice smarty-choice-override">' + A.certifyMessage + "</a>";
            }
            X += "</div></div>";
            g(X).hide().appendTo("body").show(p.speed);
            Y.selectors = {
                useOriginal: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-choice-override ",
                abort: ".smarty-popup.smarty-addr-" + Z.id() + " .smarty-abort"
            };
            if (g(t).scrollTop() > W.top - 100 || g(t).scrollTop() < W.top - g(l).height() + 100) {
                g("html, body").stop().animate({
                    scrollTop: g(".smarty-popup.smarty-addr-" + Z.id()).offset().top - 100
                }, 500);
            }
            I(Y.selectors.abort);
            g("body").on("click", Y.selectors.abort, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("InvalidAddressRejected", aa.data);
            });
            I(Y.selectors.useOriginal);
            g("body").on("click", Y.selectors.useOriginal, Y, function (aa) {
                O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                delete aa.data.selectors;
                r("OriginalInputSelected", aa.data);
            });
            g(t).keyup(Y, function (aa) {
                if (aa.keyCode == 27) {
                    I(aa.data.selectors);
                    g(Y.selectors.abort).click();
                    O(".smarty-popup.smarty-addr-" + aa.data.address.id(), aa);
                }
            });
        };
    }
    var n = {
        ALABAMA: "AL",
        ALASKA: "AK",
        "AMERICAN SAMOA": "AS",
        ARIZONA: "AZ",
        ARKANSAS: "AR",
        CALIFORNIA: "CA",
        COLORADO: "CO",
        CONNECTICUT: "CT",
        DELAWARE: "DE",
        "DISTRICT OF COLUMBIA": "DC",
        "FEDERATED STATES OF MICRONESIA": "FM",
        FLORIDA: "FL",
        GEORGIA: "GA",
        GUAM: "GU",
        HAWAII: "HI",
        IDAHO: "ID",
        ILLINOIS: "IL",
        INDIANA: "IN",
        IOWA: "IA",
        KANSAS: "KS",
        KENTUCKY: "KY",
        LOUISIANA: "LA",
        MAINE: "ME",
        "MARSHALL ISLANDS": "MH",
        MARYLAND: "MD",
        MASSACHUSETTS: "MA",
        MICHIGAN: "MI",
        MINNESOTA: "MN",
        MISSISSIPPI: "MS",
        MISSOURI: "MO",
        MONTANA: "MT",
        NEBRASKA: "NE",
        NEVADA: "NV",
        "NEW HAMPSHIRE": "NH",
        "NEW JERSEY": "NJ",
        "NEW MEXICO": "NM",
        "NEW YORK": "NY",
        "NORTH CAROLINA": "NC",
        "NORTH DAKOTA": "ND",
        "NORTHERN MARIANA ISLANDS": "MP",
        OHIO: "OH",
        OKLAHOMA: "OK",
        OREGON: "OR",
        PALAU: "PW",
        PENNSYLVANIA: "PA",
        "PUERTO RICO": "PR",
        "RHODE ISLAND": "RI",
        "SOUTH CAROLINA": "SC",
        "SOUTH DAKOTA": "SD",
        TENNESSEE: "TN",
        TEXAS: "TX",
        UTAH: "UT",
        VERMONT: "VT",
        "VIRGIN ISLANDS": "VI",
        VIRGINIA: "VA",
        WASHINGTON: "WA",
        "WEST VIRGINIA": "WV",
        WISCONSIN: "WI",
        WYOMING: "WY",
        "ARMED FORCES EUROPE, THE MIDDLE EAST, AND CANADA": "AE",
        "ARMED FORCES CANADA": "AE",
        "ARMED FORCES THE MIDDLE EAST": "AE",
        "ARMED FORCES EUROPE": "AE",
        "ARMED FORCES PACIFIC": "AP",
        "ARMED FORCES AMERICAS (EXCEPT CANADA)": "AA",
        "ARMED FORCES AMERICAS": "AA"
    };
    var q = ["ALABAMA", "ALASKA", "AMERICAN SAMOA", "ARIZONA", "ARKANSAS", "CALIFORNIA", "COLORADO", "CONNECTICUT", "DELAWARE", "DISTRICT OF COLUMBIA", "FEDERATED STATES OF MICRONESIA", "FLORIDA", "GEORGIA", "GUAM", "HAWAII", "IDAHO", "ILLINOIS", "INDIANA", "IOWA", "KANSAS", "KENTUCKY", "LOUISIANA", "MAINE", "MARSHALL ISLANDS", "MARYLAND", "MASSACHUSETTS", "MICHIGAN", "MINNESOTA", "MISSISSIPPI", "MISSOURI", "MONTANA", "NEBRASKA", "NEVADA", "NEW HAMPSHIRE", "NEW JERSEY", "NEW MEXICO", "NEW YORK", "NORTH CAROLINA", "NORTH DAKOTA", "NORTHERN MARIANA ISLANDS", "OHIO", "OKLAHOMA", "OREGON", "PALAU", "PENNSYLVANIA", "PUERTO RICO", "RHODE ISLAND", "SOUTH CAROLINA", "SOUTH DAKOTA", "TENNESSEE", "TEXAS", "UTAH", "VERMONT", "WEST VIRGINIA", "VIRGINIA", "VIRGIN ISLANDS", "WASHINGTON", "WISCONSIN", "WYOMING", "ARMED FORCES EUROPE, THE MIDDLE EAST, AND CANADA", "ARMED FORCES CANADA", "ARMED FORCES THE MIDDLE EAST", "ARMED FORCES EUROPE", "ARMED FORCES PACIFIC", "ARMED FORCES AMERICAS (EXCEPT CANADA)", "ARMED FORCES AMERICAS"];
    var u = ["AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM", "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA", "WA", "WV", "WI", "WY", "AE", "AP", "AA"];
    function w(Q, N, I, M) {
        var S = this;
        var O;
        var H;
        var G = "accepted";
        var J = function (ad, ac, aa, X, U, V) {
            if (!y(f, ad) || !Q[ad]) {
                return false;
            }
            if (!O[ad]) {
                O[ad] = {};
            }
            if (typeof O[ad].dom !== "undefined" && O[ad].dom.tagName === "SELECT" && ad !== "administrative_area" && O[ad].dom.selectedIndex >= 0) {
                ac = O[ad].dom[O[ad].dom.selectedIndex].text.replace(/<|>/g, "");
            }
            else {
                ac = ac.replace(/<|>/g, "");
            }
            var W = O[ad].value != ac;
            O[ad].undo = O[ad].value || "";
            O[ad].value = ac;
            if (O[ad].dom && aa) {
                if (O[ad].dom.tagName === "INPUT") {
                    g(O[ad].dom).val(ac);
                }
                else {
                    if (O[ad].dom.tagName === "SELECT" && ad === "administrative_area" && S.isDomestic()) {
                        var ab = "";
                        g(O[ad].dom).find("option").each(function () {
                            if (g(this).text().toUpperCase() === ac) {
                                ab = g(this).val();
                                return false;
                            }
                            else {
                                for (var ae in n) {
                                    if (n[ae] === ac && ae === g(this).text().toUpperCase()) {
                                        ab = g(this).val();
                                        return false;
                                    }
                                }
                            }
                        });
                        g(O[ad].dom).val(ab);
                    }
                }
            }
            var Y = {
                sourceEvent: U,
                field: ad,
                address: S,
                value: ac,
                suppressAutoVerification: V || false
            };
            if (W && !X) {
                s.unmarkAsValid(S);
                var Z = A.ui && A.smartyTag ? g(".smarty-ui .smarty-tag.smarty-addr-" + H) : undefined;
                if (A.target.indexOf("US") >= 0 && A.target.indexOf("INTERNATIONAL") < 0) {
                    if (S.isDomestic()) {
                        if (Z && !Z.is(":visible")) {
                            Z.show();
                        }
                        S.unaccept();
                        r("AddressChanged", Y);
                    }
                    else {
                        if (Z && Z.is(":visible")) {
                            Z.hide();
                        }
                        S.accept({
                            address: S
                        }, false);
                    }
                }
                else {
                    if (A.target.indexOf("INTERNATIONAL") >= 0 && A.target.indexOf("US") < 0) {
                        if (Z && !Z.is(":visible")) {
                            Z.show();
                        }
                        S.unaccept();
                        r("AddressChanged", Y);
                    }
                    else {
                        if (A.target.indexOf("US") >= 0 && A.target.indexOf("INTERNATIONAL") >= 0) {
                            if (Z && !Z.is(":visible")) {
                                Z.show();
                            }
                            S.unaccept();
                            r("AddressChanged", Y);
                        }
                    }
                }
            }
            return true;
        };
        this.form = N;
        this.match = M;
        this.verifyCount = 0;
        this.lastField;
        this.active = true;
        this.lastStreetInput = "";
        this.load = function (ab, V) {
            O = {};
            H = V ? V.replace(/[^a-z0-9_\-]/ig, "") : C(1, 99999);
            if (typeof ab === "object") {
                this.lastField = ab[Object.keys(ab)[Object.keys(ab).length - 1]];
                var Z = true;
                for (var U in ab) {
                    if (ab.hasOwnProperty(U)) {
                        if (!y(f, U)) {
                            continue;
                        }
                        if (typeof ab[U] == "object" && ab[U].getBoundingClientRect().top > this.lastField.getBoundingClientRect().top) {
                            this.lastField = ab[U];
                        }
                        var X, W, ad, ae;
                        try {
                            X = g(ab[U]);
                            ad = X.toArray();
                            ae = ad ? ad.length == 0 : false;
                        }
                        catch (aa) {
                            ae = true;
                        }
                        if (ae) {
                            W = ab[U] || "";
                        }
                        else {
                            W = X.val() || "";
                        }
                        O[U] = {};
                        O[U].value = W;
                        O[U].undo = W;
                        if (!ae) {
                            if (A.debug) {
                                g(X).addClass("smarty-debug-input");
                                X.attr("placeholder", U + ":" + H);
                            }
                            O[U].dom = ab[U];
                        }
                        var Y = {
                            address: this,
                            field: U,
                            value: W
                        };
                        g(t).mousedown(function (af) {
                            D = g(af.target);
                        });
                        g(t).mouseup(function (af) {
                            D = null;
                        });
                        g(t).keydown(function (af) {
                            k = g(af.target);
                        });
                        g(t).keyup(function (af) {
                            k = null;
                        });
                        !ae && g(ab[U]).change(Y, i);
                    }
                }
                var ac = this;
                g(ab.address1, ab.freeform).blur(function () {
                    s.hideAutocomplete(ac.id());
                });
                G = "changed";
            }
        };
        this.load(Q, I);
        this.set = function (X, Z, W, U, V, Y) {
            if (typeof Z !== "undefined") {
                if (typeof X === "string" && arguments.length >= 2) {
                    return J(X, Z, W, U, V, Y);
                }
                else {
                    if (typeof X === "object") {
                        var ab = true;
                        for (var aa in X) {
                            if (X.hasOwnProperty(aa)) {
                                ab = J(aa, X[aa], W, U, V, Y) ? ab : false;
                            }
                        }
                        return ab;
                    }
                }
            }
        };
        this.replaceWith = function (Z, ae, ad) {
            if (typeof Z === "array" && Z.length > 0) {
                Z = Z[0];
            }
            if (typeof Z.candidate_index != "undefined") {
                if (S.isFreeform()) {
                    var V = (Z.addressee ? Z.addressee + ", " : "") + (Z.delivery_line_1 ? Z.delivery_line_1 + ", " : "") + (Z.delivery_line_2 ? Z.delivery_line_2 + ", " : "") + (Z.components.urbanization ? Z.components.urbanization + ", " : "") + (Z.last_line ? Z.last_line : "");
                    var U = "freeform";
                    if (O.address1) {
                        U = "address1";
                    }
                    S.set(U, V, ae, true, ad, false);
                }
                else {
                    S.set("organization", Z.addressee, ae, true, ad, false);
                    S.set("address1", Z.delivery_line_1, ae, true, ad, false);
                    if (Z.delivery_line_2) {
                        S.set("address2", Z.delivery_line_2, ae, true, ad, false);
                    }
                    else {
                        S.set("address2", "", ae, true, ad, false);
                    }
                    S.set("locality", Z.components.city_name, ae, true, ad, false);
                    S.set("administrative_area", Z.components.state_abbreviation, ae, true, ad, false);
                    S.set("postal_code", Z.components.zipcode + "-" + Z.components.plus4_code, ae, true, ad, false);
                }
                S.set("address3", "", ae, true, ad, false);
                S.set("address4", "", ae, true, ad, false);
                S.set("country", "USA", ae, true, ad, false);
            }
            else {
                if (S.isFreeform()) {
                    var V = (Z.organization ? Z.organization + ", " : "") + (Z.address1 ? Z.address1 : "") + (Z.address2 ? ", " + Z.address2 : "") + (Z.address3 ? ", " + Z.address3 : "") + (Z.address4 ? ", " + Z.address4 : "") + (Z.address5 ? ", " + Z.address5 : "") + (Z.address6 ? ", " + Z.address6 : "") + (Z.address7 ? ", " + Z.address7 : "") + (Z.address8 ? ", " + Z.address8 : "") + (Z.address9 ? ", " + Z.address9 : "") + (Z.address10 ? ", " + Z.address10 : "") + (Z.address11 ? ", " + Z.address11 : "") + (Z.address12 ? ", " + Z.address12 : "");
                    var aa = Z.components.country_iso_3 ? Z.components.country_iso_3 : "";
                    S.set("freeform", V, ae, true, ad, false);
                    S.set("country", aa, ae, true, ad, false);
                }
                else {
                    S.set("organization", Z.organization, ae, true, ad, false);
                    S.set("locality", Z.components.locality, ae, true, ad, false);
                    S.set("administrative_area", Z.components.administrative_area, ae, true, ad, false);
                    if (Z.components.postal_code_short) {
                        var ac = Z.components.postal_code_short;
                        if (Z.components.postal_code_extra) {
                            ac = ac + "-" + Z.components.postal_code_extra;
                        }
                        S.set("postal_code", ac, ae, true, ad, false);
                    }
                    P(Z);
                    if (this.getDomFields().address4) {
                        S.set("address1", Z.address1, ae, true, ad, false);
                        S.set("address2", Z.address2, ae, true, ad, false);
                        S.set("address3", Z.address3, ae, true, ad, false);
                        var W = Z.address4;
                        W = T(W, Z.address5, Z.address6);
                        W = T(W, Z.address6, Z.address7);
                        W = T(W, Z.address7, Z.address8);
                        W = T(W, Z.address8, Z.address9);
                        W = T(W, Z.address9, Z.address10);
                        W = T(W, Z.address10, Z.address11);
                        W = T(W, Z.address11, Z.address12);
                        S.set("address4", W, ae, true, ad, false);
                    }
                    else {
                        if (this.getDomFields().address3) {
                            S.set("address1", Z.address1, ae, true, ad, false);
                            S.set("address2", Z.address2, ae, true, ad, false);
                            var X = Z.address3;
                            X = T(X, Z.address4, Z.address5);
                            X = T(X, Z.address5, Z.address6);
                            X = T(X, Z.address6, Z.address7);
                            X = T(X, Z.address7, Z.address8);
                            X = T(X, Z.address8, Z.address9);
                            X = T(X, Z.address9, Z.address10);
                            X = T(X, Z.address10, Z.address11);
                            X = T(X, Z.address11, Z.address12);
                            S.set("address3", X, ae, true, ad, false);
                        }
                        else {
                            if (this.getDomFields().address2) {
                                S.set("address1", Z.address1, ae, true, ad, false);
                                var Y = Z.address2;
                                Y = T(Y, Z.address3, Z.address4);
                                Y = T(Y, Z.address4, Z.address5);
                                Y = T(Y, Z.address5, Z.address6);
                                Y = T(Y, Z.address6, Z.address7);
                                Y = T(Y, Z.address7, Z.address8);
                                Y = T(Y, Z.address8, Z.address9);
                                Y = T(Y, Z.address9, Z.address10);
                                Y = T(Y, Z.address10, Z.address11);
                                Y = T(Y, Z.address11, Z.address12);
                                S.set("address2", Y, ae, true, ad, false);
                            }
                            else {
                                if (this.getDomFields().address1) {
                                    var ab = Z.address1;
                                    ab = T(ab, Z.address2, Z.address3);
                                    ab = T(ab, Z.address3, Z.address4);
                                    ab = T(ab, Z.address4, Z.address5);
                                    ab = T(ab, Z.address5, Z.address6);
                                    ab = T(ab, Z.address6, Z.address7);
                                    ab = T(ab, Z.address7, Z.address8);
                                    ab = T(ab, Z.address8, Z.address9);
                                    ab = T(ab, Z.address9, Z.address10);
                                    ab = T(ab, Z.address10, Z.address11);
                                    ab = T(ab, Z.address11, Z.address12);
                                    S.set("address1", ab, ae, true, ad, false);
                                }
                            }
                        }
                    }
                    S.set("country", Z.components.country_iso_3, ae, true, ad, false);
                }
            }
        };
        var L = function (V, U, Z, Y) {
            if (V.indexOf(U) !== -1) {
                var W = new RegExp(Y.components[U], "g");
                var X = Y[Z].replace(W, "");
                Y[Z] = X;
            }
        };
        var F = function (Y, X) {
            var Z = X[Y];
            V();
            U();
            W();
            X[Y] = Z;
            function V() {
                Z = Z.replace(/^\s+/g, "");
            }
            function U() {
                Z = Z.replace(/\s+$/g, "");
            }
            function W() {
                Z = Z.replace(/\s+/g, " ");
            }
        };
        var K = function (W) {
            for (var U = 12; U >= 1; U--) {
                var V = "address" + U;
                if (W.hasOwnProperty(V) && W[V] !== "") {
                    W[V] = "";
                    return;
                }
            }
        };
        var P = function (Z) {
            if (Z.metadata.hasOwnProperty("address_format")) {
                var aa = Z.metadata.address_format.split("|");
                for (var U in aa) {
                    var X = ["locality", "administrative_area", "postal_code", "country"];
                    var V = aa[U];
                    var W = parseInt(U) + 1;
                    var Y = "address" + W;
                    X.map(function (ab) {
                        L(V, ab, Y, Z);
                    });
                    F(Y, Z);
                }
            }
            else {
                K(Z);
            }
        };
        var T = function (U, W, V) {
            if (typeof W !== "undefined" && typeof V !== "undefined") {
                if (U !== "") {
                    U += ", ";
                }
                U += W;
            }
            return U;
        };
        this.corners = function (V) {
            var U = {};
            if (!V) {
                for (var Z in O) {
                    if (O.hasOwnProperty(Z)) {
                        if (!O[Z].dom || !g(O[Z].dom).is(":visible")) {
                            continue;
                        }
                        var Y = O[Z].dom;
                        var X = g(Y).offset();
                        X.right = X.left + g(Y).outerWidth(false);
                        X.bottom = X.top + g(Y).outerHeight(false);
                        U.top = !U.top ? X.top : Math.min(U.top, X.top);
                        U.left = !U.left ? X.left : Math.min(U.left, X.left);
                        U.right = !U.right ? X.right : Math.max(U.right, X.right);
                        U.bottom = !U.bottom ? X.bottom : Math.max(U.bottom, X.bottom);
                    }
                }
            }
            else {
                var W = g(S.lastField);
                U = W.offset();
                U.right = U.left + W.outerWidth(false);
                U.bottom = U.top + W.outerHeight(false);
            }
            U.width = U.right - U.left;
            U.height = U.bottom - U.top;
            return U;
        };
        this.verify = function (V, Z) {
            if (!S.enoughInput()) {
                return r("AddressWasMissingInput", {
                    address: S,
                    invoke: V,
                    invokeFn: Z,
                    response: new b([])
                });
            }
            s.disableFields(S);
            S.verifyCount++;
            var U = S.toRequestIntl();
            var W = A.token ? "auth-id=" + encodeURIComponent(A.key) + "&auth-token=" + encodeURIComponent(A.token) : "auth-id=" + encodeURIComponent(A.key);
            var Y = A.requestUrlInternational;
            var ab = {};
            if (S.isDomestic() && A.target.indexOf("US") >= 0) {
                Y = A.requestUrlUS;
                U = S.toRequestUS();
            }
            var X = "&agent=" + encodeURIComponent("smartystreets (plugin:website@" + z.version + ")");
            if (A.agent) {
                X += "&agent=" + encodeURIComponent(A.agent);
            }
            var aa = {
                url: Y + "?" + W + X,
                contentType: "jsonp",
                data: U,
                timeout: A.timeout
            };
            g.ajax(g.extend({}, A.ajaxSettings, aa)).done(function (ac, ad, ae) {
                r("ResponseReceived", {
                    address: S,
                    response: new b(ac),
                    invoke: V,
                    invokeFn: Z
                });
            }).fail(function (ad, ac) {
                var ae = -1;
                if (ad.responseText) {
                    ae = ad.responseText.split("\n")[0].indexOf("country");
                }
                if (ad.status === 422 && ae > -1) {
                    return r("CountryWasInvalid", {
                        address: S,
                        response: new b([]),
                        invoke: V,
                        invokeFn: Z
                    });
                }
                else {
                    r("RequestTimedOut", {
                        address: S,
                        status: ac,
                        invoke: V,
                        invokeFn: Z
                    });
                }
                S.verifyCount--;
            });
            r("RequestSubmitted", {
                address: S
            });
        };
        this.enoughInput = function () {
            var U;
            if (O.administrative_area) {
                U = O.administrative_area.value;
                if (O.administrative_area.dom !== undefined && O.administrative_area.dom.length !== undefined) {
                    if (O.administrative_area.dom.selectedIndex < 1) {
                        U = "";
                    }
                    else {
                        U = O.administrative_area.dom.options[O.administrative_area.dom.selectedIndex].text;
                    }
                }
            }
            if (O.country && !O.country.value) {
                return false;
            }
            if (O.freeform) {
                return O.freeform.value;
            }
            if (O.address1 && !O.address1.value) {
                return false;
            }
            var V = !!(O.postal_code && O.postal_code.value);
            var X = !!(O.administrative_area && U.length > 0);
            var W = !!(O.locality && O.locality.value);
            return V || (W && X);
        };
        this.toRequestIntl = function () {
            var W = {};
            if (O.hasOwnProperty("freeform") && O.hasOwnProperty("address1") && O.hasOwnProperty("locality") && O.hasOwnProperty("administrative_area") && O.hasOwnProperty("postal_code")) {
                delete O.address1;
                delete O.locality;
                delete O.administrative_area;
                delete O.postal_code;
            }
            for (var V in O) {
                if (O.hasOwnProperty(V)) {
                    var U = {};
                    if (O[V].dom && O[V].dom.tagName === "SELECT" && O[V].dom.selectedIndex >= 0) {
                        U[V] = O[V].dom[O[V].dom.selectedIndex].text;
                    }
                    else {
                        U[V] = O[V].value.replace(/\r|\n/g, " ");
                    }
                    g.extend(W, U);
                }
            }
            W.geocode = A.geocode;
            return W;
        };
        this.toRequestUS = function () {
            var U = {};
            if (O.address1 && O.address1.dom && O.address1.dom.value) {
                U.street = O.address1.dom.value;
            }
            else {
                if (O.address1 && O.address1.value) {
                    U.street = O.address1.value;
                }
            }
            if (O.address2 && O.address2.dom && O.address2.dom.value) {
                U.street2 = O.address2.dom.value;
            }
            if (O.address3 && O.address3.dom && O.address3.dom.value) {
                if (typeof U.street2 === "undefined") {
                    U.street2 = O.address3.dom.value;
                }
                else {
                    U.street2 = U.street2 += ", " + O.address3.dom.value;
                }
            }
            if (O.address4 && O.address4.dom && O.address4.dom.value) {
                if (typeof U.street2 === "undefined") {
                    U.street2 = O.address4.dom.value;
                }
                else {
                    U.street2 = U.street2 += ", " + O.address4.dom.value;
                }
            }
            if (O.locality && O.locality.dom) {
                if (O.locality.dom.tagName === "SELECT" && O.locality.dom.selectedIndex >= 0) {
                    U.city = O.locality.dom[O.locality.dom.selectedIndex].text;
                }
                else {
                    U.city = O.locality.dom.value;
                }
            }
            if (O.administrative_area && O.administrative_area.dom.value) {
                if (O.administrative_area.dom.tagName === "SELECT" && O.administrative_area.dom.selectedIndex >= 0) {
                    U.state = O.administrative_area.dom[O.administrative_area.dom.selectedIndex].text;
                }
                else {
                    U.state = O.administrative_area.dom.value;
                }
            }
            if (O.postal_code && O.postal_code.dom.value) {
                U.zipcode = O.postal_code.dom.value;
            }
            if (O.freeform && O.freeform.dom.value) {
                U.street = O.freeform.dom.value;
            }
            if (typeof this.secondary !== "undefined") {
                U.secondary = this.secondary;
                delete this.secondary;
            }
            if (typeof this.address1 !== "undefined") {
                U.street = this.address1;
                delete this.address1;
                if (typeof this.zipcode !== "undefined") {
                    U.zipcode = this.zipcode;
                    delete this.zipcode;
                }
                delete U.freeform;
            }
            U.match = this.match;
            U.candidates = A.candidates;
            return U;
        };
        function R(U) {
            if (U.dom) {
                if (U.dom.tagName !== "SELECT") {
                    return U.dom.value + " ";
                }
                else {
                    if (U.dom.selectedIndex > 0) {
                        return U.dom[U.dom.selectedIndex].text + " ";
                    }
                }
            }
            return "";
        }
        this.toString = function () {
            if (O.freeform) {
                return (O.freeform ? O.freeform.value + " " : "") + (O.country ? O.country.value : "");
            }
            else {
                var U = (O.address1 ? O.address1.value + " " : "") + (O.address2 ? O.address2.value + " " : "") + (O.address3 ? O.address3.value + " " : "") + (O.address4 ? O.address4.value + " " : "") + (O.locality ? O.locality.value + " " : "");
                if (O.administrative_area) {
                    U += R(O.administrative_area);
                }
                U += (O.postal_code ? O.postal_code.value + " " : "");
                if (O.country) {
                    U += R(O.country);
                }
                return U;
            }
        };
        this.abort = function (V, U) {
            U = typeof U === "undefined" ? false : U;
            if (!U) {
                S.unaccept();
            }
            delete S.form.processing;
            return v(V);
        };
        this.isFreeform = function () {
            return O.freeform;
        };
        this.get = function (U) {
            return O[U] ? O[U].value : null;
        };
        this.undo = function (U) {
            U = typeof U === "undefined" ? true : U;
            for (var V in O) {
                if (O.hasOwnProperty(V)) {
                    this.set(V, O[V].undo, U, false, undefined, true);
                }
            }
        };
        this.accept = function (V, U) {
            U = typeof U === "undefined" ? true : U;
            G = "accepted";
            s.enableFields(S);
            if (U) {
                s.markAsValid(S);
            }
            r("AddressAccepted", V);
        };
        this.unaccept = function () {
            G = "changed";
            s.unmarkAsValid(S);
            return S;
        };
        this.getUndoValue = function (U) {
            return O[U].undo;
        };
        this.status = function () {
            return G;
        };
        this.getDomFields = function () {
            var V = {};
            for (var W in O) {
                if (O.hasOwnProperty(W)) {
                    var U = {};
                    U[W] = O[W].dom;
                    g.extend(V, U);
                }
            }
            return V;
        };
        this.hasDomFields = function () {
            for (var U in O) {
                if (O.hasOwnProperty(U) && O[U].dom) {
                    return true;
                }
            }
        };
        this.isDomestic = function () {
            var U = "";
            if (O.country && O.country.dom) {
                U = g(O.country.dom).val();
                var W = g(O.country.dom).children(":selected");
                if (W.length > 0 && W.index() > 0) {
                    U = W.text();
                }
            }
            U = U.toUpperCase().replace(/\.|\s|\(|\)|\\|\/|-/g, "");
            var V = ["", "0", "1", "US", "USA", "USOFA", "USOFAMERICA", "AMERICAN", "UNITEDSTATES", "UNITEDSTATESAMERICA", "UNITEDSTATESOFAMERICA", "AMERICA", "840", "223", "AMERICAUNITEDSTATES", "AMERICAUS", "AMERICAUSA", "UNITEDSTATESUS", "AMERICANSAMOA", "AMERIKASĀMOA", "AMERIKASAMOA", "ASM", "MICRONESIA", "FEDERALSTATESOFMICRONESIA", "FEDERATEDSTATESOFMICRONESIA", "FSM", "GUAM", "GM", "MARSHALLISLANDS", "MHL", "NORTHERNMARIANAISLANDS", "NMP", "PALAU", "REPUBLICOFPALAU", "BELAU", "PLW", "PUERTORICO", "COMMONWEALTHOFPUERTORICO", "PRI", "UNITEDSTATESVIRGINISLANDS", "VIR"];
            return y(V, U) || O.country.value == "-1";
        };
        this.autocompleteVisible = function () {
            return A.ui && A.autocomplete && g(".smarty-autocomplete.smarty-addr-" + S.id()).is(":visible");
        };
        this.id = function () {
            return H;
        };
    }
    function j(F) {
        this.addresses = [];
        this.dom = F;
        this.activeAddressesNotAccepted = function () {
            var H = [];
            for (var G = 0; G < this.addresses.length; G++) {
                var I = this.addresses[G];
                if (I.status() != "accepted" && I.active) {
                    H.push(I);
                }
            }
            return H;
        };
        this.allActiveAddressesAccepted = function () {
            return this.activeAddressesNotAccepted().length == 0;
        };
    }
    function b(F) {
        var H = function (I) {
            if (I >= F.length || I < 0) {
                if (F.length == 0) {
                    throw new Error("Candidate index is out of bounds (no candidates returned; requested " + I + ")");
                }
                else {
                    throw new Error("Candidate index is out of bounds (" + F.length + " candidates; indicies 0 through " + (F.length - 1) + " available; requested " + I + ")");
                }
            }
        };
        var G = function (I) {
            return typeof I === "undefined" ? 0 : I;
        };
        this.raw = F;
        this.length = F.length;
        this.isValid = function () {
            return (this.length === 1 && (this.raw[0].analysis.verification_status === "Verified" || this.raw[0].analysis.verification_status === "Partial" || (typeof this.raw[0].analysis.dpv_match_code !== "undefined" && this.raw[0].analysis.dpv_match_code !== "N")));
        };
        this.isInvalid = function () {
            return (this.length === 0 || (this.length === 1 && (this.raw[0].analysis.verification_status === "None" || this.raw[0].analysis.address_precision === "None" || this.raw[0].analysis.address_precision === "AdministrativeArea" || this.raw[0].analysis.address_precision === "Locality" || this.raw[0].analysis.address_precision === "Thoroughfare" || this.raw[0].analysis.dpv_match_code === "N" || (typeof this.raw[0].analysis.verification_status === "undefined" && typeof this.raw[0].analysis.dpv_match_code === "undefined"))));
        };
        this.isAmbiguous = function () {
            return this.length > 1;
        };
        this.isMissingSecondary = function (I) {
            I = G(I);
            H(I);
            return (this.raw[I].analysis.dpv_footnotes && this.raw[I].analysis.dpv_footnotes.indexOf("N1") > -1) || (this.raw[I].analysis.dpv_footnotes && this.raw[I].analysis.dpv_footnotes.indexOf("R1") > -1) || (this.raw[I].analysis.footnotes && this.raw[I].analysis.footnotes.indexOf("H#") > -1);
        };
    }
    function a(G, H) {
        var F = z.events[G.type];
        if (F) {
            F(G, H);
        }
    }
    var D = null;
    var k = null;
    function i(K) {
        function M() {
            return D === null;
        }
        function G() {
            return D[0].tagName === "B";
        }
        function H(N) {
            return N.className === "smarty-suggestion smarty-active-suggestion";
        }
        function L() {
            return k === null;
        }
        function J() {
            return !M() && G() && !H(D[0].parentElement);
        }
        function I() {
            return !M() && !G() && !H(D[0]);
        }
        function F() {
            return M() && L() && K.data.address.autocompleteVisible();
        }
        if (J() || I() || F()) {
            s.hideAutocomplete(K.data.address.id());
        }
        K.data.address.set(K.data.field, K.target.value, false, false, K, false);
    }
    var o = function (F, G) {
        if (F && typeof F !== "function" && G) {
            if (G == "click") {
                setTimeout(function () {
                    g(F).click();
                }, 5);
            }
            else {
                if (G == "submit") {
                    g(F).submit();
                }
            }
        }
    };
    function y(H, G) {
        for (var F in H) {
            if (H.hasOwnProperty(F) && H[F] === G) {
                return true;
            }
        }
        return false;
    }
    function C(G, F) {
        return Math.floor(Math.random() * (F - G + 1)) + G;
    }
    function h(F) {
        return F ? F.toLowerCase().replace("[]", "") : "";
    }
    function r(G, F) {
        g(t).triggerHandler(G, F);
    }
    function x(F) {
        g(t).on(F, a);
    }
    function v(F) {
        if (!F) {
            return false;
        }
        if (F.preventDefault) {
            F.preventDefault();
        }
        if (F.stopPropagation) {
            F.stopPropagation();
        }
        if (F.stopImmediatePropagation) {
            F.stopImmediatePropagation();
        }
        F.cancelBubble = true;
        return false;
    }
    function c() {
        var J = "//us-street.api.smartystreets.com";
        var I = "//us-autocomplete.api.smartystreets.com";
        var K = "//international-street.api.smartystreets.com";
        var G = [J, I, K];
        for (var H = 0; H < G.length; H++) {
            var F = 'href="' + G[H] + '">';
            g("head").append(g('<link rel="preconnect" ' + F)).append('<link rel="dns-prefetch" ' + F);
        }
    }
})(jQuery, window, document);
function htmlDecode(input) {
    var doc = new DOMParser().parseFromString(input, "text/html");
    return doc.documentElement.textContent;
}
//# sourceMappingURL=liveAddress.js.map