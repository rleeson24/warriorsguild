var WarriorsGuild;
(function (WarriorsGuild) {
    var AppDataModel = /** @class */ (function () {
        function AppDataModel() {
            // Routes
            this.userInfoUrl = "/api/Profile";
            // Route operations
            // Other private operations
            // Operations
            // Data
            this.WarriorDropDownItems = warriorDropDownItems;
            this.selectedWarrior = ko.observable(selectedWarrior);
        }
        // Data access operations
        AppDataModel.prototype.setAccessToken = function (accessToken) {
            WarriorsGuild.serviceBase.setAccessToken(accessToken);
        };
        ;
        AppDataModel.prototype.getAccessToken = function () {
            return WarriorsGuild.serviceBase.getAccessToken();
        };
        ;
        AppDataModel.prototype.removeAccessToken = function () {
            WarriorsGuild.serviceBase.removeAccessToken();
        };
        ;
        return AppDataModel;
    }());
    WarriorsGuild.AppDataModel = AppDataModel;
    var Warrior = /** @class */ (function () {
        function Warrior() {
        }
        return Warrior;
    }());
    WarriorsGuild.Warrior = Warrior;
    var Guardian = /** @class */ (function () {
        function Guardian() {
        }
        return Guardian;
    }());
    WarriorsGuild.Guardian = Guardian;
    var ApplicationUser = /** @class */ (function () {
        function ApplicationUser() {
        }
        return ApplicationUser;
    }());
    WarriorsGuild.ApplicationUser = ApplicationUser;
    var ObservableUser = /** @class */ (function () {
        function ObservableUser() {
            this.userName = ko.observable('');
            this.firstName = ko.observable('');
            this.lastName = ko.observable('');
            this.addressLine1 = ko.observable('');
            this.addressLine2 = ko.observable('');
            this.city = ko.observable('');
            this.state = ko.observable('');
            this.postalCode = ko.observable('');
            this.phoneNumber = ko.observable('');
            this.shirtSize = ko.observable('');
            this.email = ko.observable('');
            this.id = ko.observable('');
        }
        return ObservableUser;
    }());
    WarriorsGuild.ObservableUser = ObservableUser;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=app.datamodel.js.map