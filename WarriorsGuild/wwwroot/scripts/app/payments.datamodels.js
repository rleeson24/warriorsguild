var WarriorsGuild;
(function (WarriorsGuild) {
    var Payments;
    (function (Payments) {
        var ManageablePriceOption = /** @class */ (function () {
            function ManageablePriceOption() {
                this.id = '';
                this.name = '';
                this.description = '';
                this.charge = 0;
                this.setupFee = 0;
                this.numberOfGuardians = 0;
                this.numberOfWarriors = 0;
                this.additionalGuardianCharge = 0;
                this.additionalWarriorCharge = 0;
                this.stripeStatus = WarriorsGuild.WgStripeStatuses.STATUS_OPTION_INCOMPLETE;
                this.hasTrialPeriod = false;
                this.perks = [];
            }
            return ManageablePriceOption;
        }());
        Payments.ManageablePriceOption = ManageablePriceOption;
        var SubscribeablePriceOption = /** @class */ (function () {
            function SubscribeablePriceOption() {
                this.id = '';
                this.description = '';
                this.charge = 0;
                this.setupFee = 0;
                this.numberOfGuardians = 0;
                this.numberOfWarriors = 0;
                this.hasTrialPeriod = false;
                this.perks = [];
            }
            return SubscribeablePriceOption;
        }());
        Payments.SubscribeablePriceOption = SubscribeablePriceOption;
        var AddOnPriceOption = /** @class */ (function () {
            function AddOnPriceOption() {
                this.description = '';
                this.frequency = 0;
                this.charge = 0;
                this.currency = 'usd';
                this.trialPeriodLength = 0;
                this.numberOfGuardians = 0;
                this.numberOfWarriors = 0;
            }
            return AddOnPriceOption;
        }());
        Payments.AddOnPriceOption = AddOnPriceOption;
        var Subscription = /** @class */ (function () {
            function Subscription() {
                //cancelled
                //status
                this.additionalGuardians = 0;
                this.additionalCostPerGuardian = 0;
                this.additionalWarriors = 0;
                this.additionalCostPerWarrior = 0;
                this.paymentMethod = 0;
                this.charge = 0;
                //show
                this.trialPeriodLength = 0;
                this.numberOfGuardians = 0;
                this.numberOfWarriors = 0;
                this.setupFee = 0;
                //hasTrialPeriod
                this.perks = [];
                this.users = [];
            }
            return Subscription;
        }());
        Payments.Subscription = Subscription;
        var ReadOnlySubscription = /** @class */ (function () {
            function ReadOnlySubscription() {
                //cancelled
                //status
                this.additionalGuardians = 0;
                this.additionalWarriors = 0;
                this.paymentMethod = 0;
                //show
                this.trialPeriodLength = 0;
                this.numberOfGuardians = 0;
                this.numberOfWarriors = 0;
                //hasTrialPeriod
                this.perks = [];
                this.guardianUsers = [];
                this.warriorUsers = [];
            }
            return ReadOnlySubscription;
        }());
        Payments.ReadOnlySubscription = ReadOnlySubscription;
        var Frequency;
        (function (Frequency) {
            Frequency[Frequency["Monthly"] = 0] = "Monthly";
            Frequency[Frequency["Yearly"] = 1] = "Yearly";
        })(Frequency = Payments.Frequency || (Payments.Frequency = {}));
        var SubscriptionUser = /** @class */ (function () {
            function SubscriptionUser() {
            }
            return SubscriptionUser;
        }());
        Payments.SubscriptionUser = SubscriptionUser;
        var Perk = /** @class */ (function () {
            function Perk() {
            }
            return Perk;
        }());
        Payments.Perk = Perk;
    })(Payments = WarriorsGuild.Payments || (WarriorsGuild.Payments = {}));
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=payments.datamodels.js.map