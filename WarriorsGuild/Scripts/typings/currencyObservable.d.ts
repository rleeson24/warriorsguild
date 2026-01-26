

interface KnockoutCustomObservables {
	currencyComputed<T>( number, context?: any ): CurrencyObservable<T>;
	currencyComputed<T>(evaluatorFunction: () => T, context?: any): CurrencyObservable<T>;
	currencyComputed<T>(options: KnockoutComputedDefine<T>, context?: any): CurrencyObservable<T>;
	numericComputed<T>(number, context?: any): NumericObservable<T>;
	numericComputed<T>(evaluatorFunction: () => T, context?: any): NumericObservable<T>;
	numericComputed<T>(options: KnockoutComputedDefine<T>, context?: any): NumericObservable<T>;
	phoneComputed<T>(number, context?: any): PhoneObservable<T>;
	phoneComputed<T>(evaluatorFunction: () => T, context?: any): PhoneObservable<T>;
	phoneComputed<T>(options: KnockoutComputedDefine<T>, context?: any): PhoneObservable<T>;
}

interface CurrencyObservable<T> extends KnockoutComputed<T>, KnockoutComputedFunctions<T> {
}

interface NumericObservable<T> extends KnockoutComputed<T>, KnockoutComputedFunctions<T> {
}

interface PhoneObservable<T> extends KnockoutComputed<T>, KnockoutComputedFunctions<T> {
}

declare var koCustom: KnockoutCustomObservables;