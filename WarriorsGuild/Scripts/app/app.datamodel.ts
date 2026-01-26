declare var warriorDropDownItems: WarriorsGuild.Warrior[];
declare var selectedWarrior: WarriorsGuild.Warrior;

namespace WarriorsGuild {
    export class AppDataModel {
       
        userInfoUrl: string;
		WarriorDropDownItems: Warrior[];
		selectedWarrior: KnockoutObservable<Warrior>;
		
        constructor() {
            // Routes
            this.userInfoUrl = "/api/Profile";
        
            // Route operations

            // Other private operations

            // Operations

            // Data
			this.WarriorDropDownItems = warriorDropDownItems;
			this.selectedWarrior = ko.observable( selectedWarrior );
        }
        // Data access operations
        setAccessToken(accessToken): void {
			WarriorsGuild.serviceBase.setAccessToken(accessToken);
		};

		getAccessToken(): string {
			return WarriorsGuild.serviceBase.getAccessToken();
		};

		removeAccessToken(): void {
			WarriorsGuild.serviceBase.removeAccessToken();
		};
	}

	export class Warrior {
		id: string;
		name: string;
		username: string;
		avatarSrc: string;
	}

	export class Guardian {
		id: string;
		name: string;
		username: string;
		hasAvatar: boolean;
		subscriptionDescription: string;
		subscriptionExpires: string;
	}

	export class ApplicationUser {
		userName: string;
		firstName: string;
		lastName: string;
		addressLine1: string;
		addressLine2: string;
		city: string;
		state: string;
		postalCode: string;
		phoneNumber: string;
		shirtSize: string;
		email: string;
		id: string;
	}

	export class ObservableUser {
		userName: KnockoutObservable<string> = ko.observable<string>( '' );
		firstName: KnockoutObservable<string> = ko.observable( '' );
		lastName: KnockoutObservable<string> = ko.observable( '' );
		addressLine1: KnockoutObservable<string> = ko.observable( '' );
		addressLine2: KnockoutObservable<string> = ko.observable( '' );
		city: KnockoutObservable<string> = ko.observable( '' );
		state: KnockoutObservable<string> = ko.observable( '' );
		postalCode: KnockoutObservable<string> = ko.observable( '' );
		phoneNumber: KnockoutObservable<string> = ko.observable( '' );
		shirtSize: KnockoutObservable<string> = ko.observable( '' );
		email: KnockoutObservable<string> = ko.observable( '' );
		id: KnockoutObservable<string> = ko.observable( '' );
	}
}