declare var paymentUrls: {
	priceOptionsUrl: string;
	subscriptionsUrl: string;
};
declare var priceOptionStates: string[];

namespace WarriorsGuild {
	export class PaymentPlansViewModel {
		app: AppViewModel;
		showSaveSuccess: KnockoutObservable<boolean>;
		showDeleteSuccess: KnockoutObservable<boolean>;
		showSaveFailure: KnockoutObservable<boolean>;
		showDeleteFailure: KnockoutObservable<boolean>;
		actionFailureMessage: KnockoutObservable<string>;
		actionSuccessMessage: KnockoutObservable<string>;
		dataModel: {
			PriceOptionsUrl: string;
			PriceOptions: KnockoutObservableArray<ObservablePriceOption>;
			Name: KnockoutObservable<string>;
			Description: KnockoutObservable<string>;
			Frequency: KnockoutObservable<number>;
			Charge: CurrencyObservable<number>;
			SetupFee: CurrencyObservable<number>;
			NumberOfGuardians: KnockoutObservable<number>;
			NumberOfWarriors: KnockoutObservable<number>;
			AdditionalGuardianCharge: CurrencyObservable<number>;
			AdditionalWarriorCharge: CurrencyObservable<number>;
			HasTrialPeriod: KnockoutObservable<boolean>;
			TrialPeriodLength: KnockoutObservable<number | null>;
			TrialPeriodCharge: KnockoutObservable<number | null>;
			perks: KnockoutObservableArray<ObservablePerk>;
			RetrievingPlans: KnockoutObservable<Boolean>;
		};
		constructor(app: WarriorsGuild.AppViewModel) {
			this.app = app;
			var self = this;
			this.dataModel = {
				PriceOptionsUrl: paymentUrls.priceOptionsUrl,
				PriceOptions: ko.observableArray<ObservablePriceOption>(),
				Name: ko.observable(''),
				Description: ko.observable(''),
				Frequency: ko.observable(0),
				Charge: koCustom.currencyComputed<number>(0),
				SetupFee: koCustom.currencyComputed<number>(0),
				NumberOfGuardians: koCustom.numericComputed<number>(0),
				NumberOfWarriors: koCustom.numericComputed<number>(0),
				AdditionalGuardianCharge: koCustom.currencyComputed<number>(0),
				AdditionalWarriorCharge: koCustom.currencyComputed<number>(0),
				HasTrialPeriod: ko.observable(false),
				TrialPeriodLength: koCustom.numericComputed<number | null>(null),
				TrialPeriodCharge: koCustom.currencyComputed<number | null>(null),
				perks: ko.observableArray([]),
				RetrievingPlans: ko.observable(false)
			};
			this.showDeleteSuccess = ko.observable(false);
			this.showSaveSuccess = ko.observable(false);
			this.showDeleteFailure = ko.observable(false);
			this.showSaveFailure = ko.observable(false);
			this.actionFailureMessage = ko.observable('');
			this.actionSuccessMessage = ko.observable('');
			app.prepareAjax();

			Sammy(function () {
				this.get('#priceOptions', function () {
					self.HideAlerts();
					self.dataModel.RetrievingPlans(true);
					// Make a call to the protected Web API by passing in a Bearer Authorization Header
					$.ajax({
						method: 'get',
						url: self.dataModel.PriceOptionsUrl,
						contentType: "application/json; charset=utf-8",
						headers: {
							'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
						},
						success: function (data: WarriorsGuild.Payments.ManageablePriceOption[]) {
							self.dataModel.PriceOptions.removeAll();
							$.each(data, function (i, d) {
								var mappedPO = self.CreateObservablePriceOption(d);
								self.dataModel.PriceOptions.push(mappedPO);
							});
							self.dataModel.RetrievingPlans(false);
						},
						error: function (err: JQueryXHR) {
							self.actionFailureMessage('The Price Options could not be retrieved');
							self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
							self.showSaveFailure(true);
							self.dataModel.RetrievingPlans(false);
						},
					});
				});
				this.get('/ManagePriceOptions', function () { this.app.runRoute('get', '#priceOptions'); });
			});
		}

		CreateObservablePriceOption = (d: WarriorsGuild.Payments.ManageablePriceOption): ObservablePriceOption => {
			var self = this;
			var mappedPO = <ObservablePriceOption><any>ko.mapping.fromJS(d, <KnockoutMappingOptions<WarriorsGuild.Payments.ManageablePriceOption>>{
				perks: {
					create: function (options: KnockoutMappingCreateOptions) {
						return ko.mapping.fromJS(options.data, new ObservablePerk());
					}
				}
			}, <KnockoutObservableType<WarriorsGuild.Payments.ManageablePriceOption>><any>new ObservablePriceOption());
			mappedPO.perks(mappedPO.perks() || []);
			mappedPO.charge = koCustom.currencyComputed<number>(mappedPO.charge());
			mappedPO.setupFee = koCustom.currencyComputed<number>(mappedPO.setupFee());
			mappedPO.additionalGuardianCharge = koCustom.currencyComputed<number>(mappedPO.additionalGuardianCharge());
			mappedPO.additionalWarriorCharge = koCustom.currencyComputed<number>(mappedPO.additionalWarriorCharge());
			return mappedPO;
		};

		HideAlerts = (): void => {
			this.showDeleteSuccess(false);
			this.showSaveSuccess(false);
			this.showDeleteFailure(false);
			this.showSaveFailure(false);
		};

		saveOption = (data: ObservablePriceOption, saveUrl: string): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			this.HideAlerts();

			var objectToSave: WarriorsGuild.Payments.ManageablePriceOption;
			objectToSave = <WarriorsGuild.Payments.ManageablePriceOption>ko.toJS(data);
			$.each(objectToSave.perks, function (index, value) {
				value.index = index;
				value.priceOptionId = objectToSave.id;
				if (value.id <= '') delete value["id"];
			});

			var url = '';
			var method = '';
			if (objectToSave.id > '') {
				url = saveUrl + '/' + data.id();
				method = 'put';
			}
			else {
				url = saveUrl;
				method = 'post';
			}
			$.ajax({
				method: method,
				url: url,
				data: ko.toJSON(objectToSave),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				error: function (err: JQueryXHR) {
					self.actionFailureMessage('The Price Option could not be saved');
					self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
					self.showSaveFailure(true);
				},
				success: function (d: WarriorsGuild.Payments.ManageablePriceOption) {
					if (objectToSave.id === undefined) {
						var mappedPO = self.CreateObservablePriceOption(d);
						self.dataModel.PriceOptions.push(mappedPO);
					}
					else {
						var oPo = <ObservablePriceOption><any>ko.mapping.fromJS(d,
							<KnockoutMappingOptions<WarriorsGuild.Payments.ManageablePriceOption>>{},
							<KnockoutObservableType<WarriorsGuild.Payments.ManageablePriceOption>><any>data);
						self.dataModel.PriceOptions.push(oPo);
					}
					self.showSaveSuccess(true);
				}
			});
		}

		activateOption = (data: ObservablePriceOption, saveUrl: string): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			this.HideAlerts();

			if (data.id() > '') {
				var url = saveUrl + '/' + data.id() + "/Activate";
				var method = 'put';
				$.ajax({
					method: method,
					url: url,
					contentType: "application/json; charset=utf-8",
					headers: {
						'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
					},
					error: function (err: JQueryXHR) {
						self.actionFailureMessage('The Price Option could not be activated');
						self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
						self.showSaveFailure(true);
					},
					success: function (returnData: WarriorsGuild.Payments.ManageablePriceOption) {
						data.stripeStatus(WgStripeStatuses.STATUS_OPTION_ACTIVE);
						self.showSaveSuccess(true);
					}
				});
			}
			else {
				self.actionFailureMessage('The Price Option could not be activated.  Refresh the page and try again.');
				self.showSaveFailure(true);
			}
		}

		deactivateOption = (data: ObservablePriceOption, saveUrl: string): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			this.HideAlerts();

			if (data.id() > '') {
				var url = saveUrl + '/' + data.id() + "/Deactivate";
				var method = 'put';
				$.ajax({
					method: method,
					url: url,
					contentType: "application/json; charset=utf-8",
					headers: {
						'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
					},
					error: function (err: JQueryXHR) {
						self.actionFailureMessage('The Price Option could not be deactivated');
						self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
						self.showSaveFailure(true);
					},
					success: function (returnData: WarriorsGuild.Payments.ManageablePriceOption) {
						data.stripeStatus(WgStripeStatuses.STATUS_OPTION_INACTIVE);
						self.showSaveSuccess(true);
					}
				});
			}
			else {
				self.actionFailureMessage('The Price Option could not be deactivated.  Refresh the page and try again.');
				self.showSaveFailure(true);
			}
		}

		deleteOption = (data: ObservablePriceOption, saveUrl: string): void => {
			var self = this;
			// Make a call to the protected Web API by passing in a Bearer Authorization Header
			this.HideAlerts();

			var objectToSave = new WarriorsGuild.Payments.ManageablePriceOption();
			objectToSave.id = data.id();
			var url = saveUrl + '/' + data.id();
			var method = 'delete';
			$.ajax({
				method: method,
				url: url,
				data: ko.toJSON(objectToSave),
				contentType: "application/json; charset=utf-8",
				headers: {
					'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
				},
				error: function (err: JQueryXHR) {
					self.actionFailureMessage('The Price Option could not be deleted');
					self.actionFailureMessage(self.actionFailureMessage() + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err));
					self.showSaveFailure(true);
				},
				success: function (returnData: WarriorsGuild.Payments.ManageablePriceOption) {
					if (data.planId() <= '') {
						self.dataModel.PriceOptions.remove(data);
					}
					else {
						data.stripeStatus(WgStripeStatuses.STATUS_OPTION_INCOMPLETE);
					}
					self.showSaveSuccess(true);
				}
			});
		}
		//CreateObservablePerk = ( option: PriceOption, data: Perk ): ObservablePerk => {
		//	var self = this;
		//	var result = new ObservablePerk();
		//	result.id( data.id );
		//	result.description( data.description );
		//	return result;
		//}
	}

	class PriceOptionsDataModel {
		PriceOptionsUrl: string;
		PriceOptions: KnockoutObservableArray<WarriorsGuild.Payments.ManageablePriceOption>;
	}

	class ObservablePriceOption {
		id: KnockoutObservable<string> = ko.observable('');
		planId: KnockoutObservable<string> = ko.observable('');
		key: KnockoutObservable<string> = ko.observable('');
		name: KnockoutObservable<string> = ko.observable('');
		description: KnockoutObservable<string> = ko.observable('');
		charge: CurrencyObservable<number>;
		setupFee: CurrencyObservable<number>;
		stripeStatus: KnockoutObservable<string> = ko.observable<string>(WgStripeStatuses.STATUS_OPTION_INCOMPLETE);
		numberOfGuardians: KnockoutObservable<number> = ko.observable(0);
		numberOfWarriors: KnockoutObservable<number> = ko.observable(0);
		additionalGuardianCharge: CurrencyObservable<number>;
		additionalWarriorCharge: CurrencyObservable<number>;
		hasTrialPeriod: KnockoutObservable<boolean> = ko.observable(false);
		trialPeriodLength: KnockoutObservable<number | null> = ko.observable(null);
		trialPeriodCharge: KnockoutObservable<number | null> = ko.observable(null);
		perks: KnockoutObservableArray<ObservablePerk> = ko.observableArray([]);

		stripeStatusForDisplay: KnockoutComputed<string> = ko.computed(function () { return this.stripeStatus(); }, this);

		addPerk(data: ObservablePriceOption): void {
			data.perks.push(new ObservablePerk());
		};

		removePerk = (data: ObservablePerk): void => {
			this.perks.remove(data);
		};
	}

	class ObservablePerk {
		constructor() {
			this.id = ko.observable('');
			this.description = ko.observable('');
			this.quantity = koCustom.numericComputed(null);
			this.priceOptionId = ko.observable('');
		}
		id: KnockoutObservable<string>;
		description: KnockoutObservable<string>;
		quantity: NumericObservable<number | null>;
		priceOptionId: KnockoutObservable<string>;
	}
}

WarriorsGuild.app.addViewModel({
	name: "PaymentPlans",
	bindingMemberName: "paymentPlans",
	factory: WarriorsGuild.PaymentPlansViewModel
});