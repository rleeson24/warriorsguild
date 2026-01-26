namespace WarriorsGuild.Payments {
	export class ManageablePriceOption {
		id: string = '';
		name: string = '';
		description: string = '';
		charge: number = 0;
		frequency: Frequency;
        setupFee: number = 0;
        numberOfGuardians: number = 0;
        numberOfWarriors: number = 0;
        additionalGuardianCharge: number = 0;
        additionalWarriorCharge: number = 0;
		stripeStatus: string = WgStripeStatuses.STATUS_OPTION_INCOMPLETE;
		hasTrialPeriod: boolean = false;
		trialPeriodLength: number | null;
		trialPeriodCharge: number | null;
		perks: Perk[] = [];
	}

	export class SubscribeablePriceOption {
		id: string = '';
		description: string = '';
		charge: number = 0;
		setupFee: number = 0;
		numberOfGuardians: number = 0;
		numberOfWarriors: number = 0;
		additionalGuardianPlan: AddOnPriceOption;
		additionalWarriorPlan: AddOnPriceOption;
		hasTrialPeriod: boolean = false;
		trialPeriodLength: number | null;
		perks: Perk[] = [];
	}

	export class AddOnPriceOption {
		description: string = '';
		frequency: number = 0;
		charge: number = 0;
		currency: string = 'usd';
		trialPeriodLength: number = 0;
		numberOfGuardians: number = 0;
		numberOfWarriors: number = 0;
		quantity: KnockoutObservable<number>;
	}

	export class Subscription {
		dateCreated: string;
        lastPaid: string;
        nextPaymentDue: string;
		//cancelled
		//status
		additionalGuardians: number = 0;
		additionalCostPerGuardian: number = 0;
		additionalWarriors: number = 0;
		additionalCostPerWarrior: number = 0;
		paymentMethod: number = 0;
		stripeSubscriptionId: string;


		priceOptionId: string;
		//key
		description: string;
		frequency: Frequency;
		charge: number = 0;
		currency: string;
		//show
		trialPeriodLength: number = 0;
		numberOfGuardians: number = 0;
		numberOfWarriors: number = 0;
		setupFee: number = 0;
		//hasTrialPeriod
		perks: Perk[] = [];
		users: SubscriptionUser[] = [];
	}

	export class ReadOnlySubscription {
		dateCreated: Date;
		lastPaid: Date;
		nextPaymentDue: Date;
		//cancelled
		//status
		additionalGuardians: number = 0;
		additionalCostPerGuardian: CurrencyObservable<number>;
		totalAdditionalGuardianCost: CurrencyObservable<number>;
		additionalWarriors: number = 0;
		additionalCostPerWarrior: CurrencyObservable<number>;
		totalAdditionalWarriorCost: CurrencyObservable<number>;
		paymentMethod: number = 0;
		stripeSubscriptionId: string;
		totalPeriodicCharge: CurrencyObservable<number>;

		priceOptionId: string;
		//key
		description: string;
		frequency: Frequency;
		charge: CurrencyObservable<number>;
		currency: string;
		//show
		trialPeriodLength: number = 0;
		numberOfGuardians: number = 0;
		numberOfWarriors: number = 0;
		setupFee: CurrencyObservable<number>;
		//hasTrialPeriod
		perks: Perk[] = [];
		guardianUsers: SubscriptionUser[] = [];
		warriorUsers: SubscriptionUser[] = [];
	}

	export enum Frequency { Monthly, Yearly }

	export class SubscriptionUser {
		id: string;
		firstName: string;
		lastName: string;
		isWarrior: boolean;
		isGuardian: boolean;
		isPayingParty: boolean;
	}

	export class Perk {
		id: string;
		description: string;
		priceOptionId: string;
		quantity: number | null;
		index: number;
	}
}