
namespace WarriorsGuild {
    export interface Fragment {
        state: string;
		access_token: string;
		hash: string;
    }

    export class common {
        static getFragment(): Fragment;
        static getFragment(): Object;
        static getFragment(): any {
			if ( window.location.hash.indexOf( "#" ) === 0 && window.location.hash !== '#_=_') {
                return common.parseQueryString(window.location.hash.substr(1));
            } else {
                return {};
            }
        }

        static parseQueryString(queryString): Fragment;
        static parseQueryString(queryString): Object;
        static parseQueryString(queryString): any {
            var data = {},
                pairs, pair, separatorIndex, escapedKey, escapedValue, key, value;

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
                } else {
                    escapedKey = pair.substr(0, separatorIndex);
                    escapedValue = pair.substr(separatorIndex + 1);
                }

                key = decodeURIComponent(escapedKey);
                value = decodeURIComponent(escapedValue);

                data[key] = value;
            }

            return data;
        }
    }

    export class BaseRequirement {
        id: string;
        actionToComplete: string;
        warriorCompleted: string;
        guardianCompleted: string;
        warriorCompletedTs: string;
        guardianReviewedTs: string;
        weight: number;
        requireAttachment: boolean;
        attachments: [{
            id: string;
        }];
        seeHowLink: string;
    }

    export function ParseResponseErrorWithLeadingPeriod(err: JQueryXHR) {
        var response = ParseResponseError(err);
        return response > '' ? `. ${response}` : '';
	}
    export function ParseResponseError(err: JQueryXHR ) {
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

    export function isValidUrl(inputString: string): boolean {
        var expression = /[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)?/gi;
        var regex = new RegExp(expression);
        
        return regex.test(inputString);
    }
}