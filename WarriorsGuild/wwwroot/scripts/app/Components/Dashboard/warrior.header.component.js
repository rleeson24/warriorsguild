var WarriorsGuild;
(function (WarriorsGuild) {
    var WarriorHeaderViewModel = /** @class */ (function () {
        function WarriorHeaderViewModel(params) {
            var _this = this;
            this.hasAvatar = ko.observable(false);
            this.username = ko.observable('');
            this.userFullName = ko.observable('');
            this.showFavoriteData = false;
            this.favoriteVerse = ko.observable('');
            this.hobbies = ko.observable('');
            this.interestingFact = ko.observable('');
            this.favoriteMovie = ko.observable('');
            this.hasFavoriteVerse = ko.pureComputed(function () { return _this.favoriteVerse() && _this.favoriteVerse().length > 0; });
            this.hasHobbies = ko.pureComputed(function () { return _this.hobbies() && _this.hobbies().length > 0; });
            this.hasInterestingFact = ko.pureComputed(function () { return _this.interestingFact() && _this.interestingFact().length > 0; });
            this.hasFavoriteMovie = ko.pureComputed(function () { return _this.favoriteMovie() && _this.favoriteMovie().length > 0; });
            this.CompletedCrosses = ko.observableArray([]);
            this.CompletedSilverRings = ko.observableArray([]);
            this.CompletedGoldRings = ko.observableArray([]);
            this.CompletedPlatinumRings = ko.observableArray([]);
            this.avatarPath = ko.pureComputed(function () {
                return '/api/Profile/' + _this.profileId + '/Avatar';
            });
            this.loadProfileData = function (warriorDashboardViewModel) {
                _this.userFullName(warriorDashboardViewModel.userFullName);
                _this.favoriteVerse(warriorDashboardViewModel.favoriteVerse);
                _this.hobbies(warriorDashboardViewModel.hobbies);
                _this.interestingFact(warriorDashboardViewModel.interestingFact);
                _this.favoriteMovie(warriorDashboardViewModel.favoriteMovie);
            };
            this.getCompletedRings = function () {
                var self = _this;
                _this.ringService.getCompletedRings(_this.profileId, function (data) {
                    var silver = 'Silver', gold = 'Gold', platinum = 'Platinum';
                    $.each(data.sort(function (a, b) { return a.name.localeCompare(b.name); }), function (i, v) {
                        var oRing = ko.mapping.fromJS(v, self.koMapperConfiguration, new WarriorsGuild.ObservableRing());
                        switch (v.type) {
                            case silver:
                                self.CompletedSilverRings.push(oRing);
                                break;
                            case gold:
                                self.CompletedGoldRings.push(oRing);
                                break;
                            case platinum:
                                self.CompletedPlatinumRings.push(oRing);
                                break;
                        }
                    });
                }, function (err) {
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving completed rings"
                        });
                    }
                });
            };
            this.getCompletedCrosses = function () {
                var self = _this;
                _this.crossService.getCompletedCrossesForUser(_this.profileId, function (data) {
                    $.each(data.sort(function (a, b) { return a.name.localeCompare(b.name); }), function (i, v) {
                        var oCross = ko.mapping.fromJS(v, self.koMapperConfiguration, new WarriorsGuild.ObservableCross());
                        self.CompletedCrosses.push(oCross);
                    });
                }, function (err) {
                    var errMessage = WarriorsGuild.ParseResponseError(err);
                    if (errMessage > '') {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: errMessage
                        });
                    }
                    else {
                        BootstrapAlert.alert({
                            title: "Retrieve Failure!",
                            message: "A problem has been occurred retrieving completed crosses"
                        });
                    }
                });
            };
            this.koMapperConfiguration = (function () {
                var self = _this;
                return {
                    'completed': {
                        update: function (options) {
                            return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                        }
                    },
                    'confirmed': {
                        update: function (options) {
                            return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                        }
                    },
                    'imageUploaded': {
                        update: function (options) {
                            return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                        }
                    },
                    'guideUploaded': {
                        update: function (options) {
                            return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                        }
                    },
                    'warriorCompletedTs': {
                        update: function (options) {
                            return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                        }
                    },
                    'guardianReviewedTs': {
                        update: function (options) {
                            return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                        }
                    },
                    'guardianApprovedTs': {
                        update: function (options) {
                            return WarriorsGuild.DateConversion.convertStringToNullableDate(options.data);
                        }
                    },
                    'requirements': {
                        create: function (options) {
                            return ko.mapping.fromJS(options.data, {}, new WarriorsGuild.ObservableRankRequirement());
                        }
                    }
                };
            })();
            this.crossService = new WarriorsGuild.CrossService();
            this.showFavoriteData = params.showFavoriteData;
            this.profileId = params.userId;
            this.rank = params.rankDetail;
            this.ringService = new WarriorsGuild.RingService();
            this.profileService = new WarriorsGuild.ProfileService();
            this.profileService.retrieveProfileData(this.profileId, this.loadProfileData);
            this.getCompletedRings();
            this.getCompletedCrosses();
        }
        return WarriorHeaderViewModel;
    }());
    WarriorsGuild.WarriorHeaderViewModel = WarriorHeaderViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('warior-header', {
    viewModel: WarriorsGuild.WarriorHeaderViewModel,
    template: "<div class=\"row\">\n                    <div class=\"col-md-offset-3 col-md-5 col-lg-4\">\n                        <div class=\"photo-and-rank\">\n                            <div class=\"user-photo\">\n                                <!-- ko if: $component.hasAvatar() -->\n                                <img data-bind=\"attr: { src: $component.avatarPath() }\" style=\"margin-top: -50px; width: 160px; height: 160px; border-radius: 50%\">\n                                <!-- /ko -->\n                                <!-- ko if: !$component.hasAvatar() -->\n                                <span data-bind=\"text: $component.username\"></span>\n                                <!-- /ko -->\n                            </div>\n                            <!-- ko with: rank() -->\n                            <div class=\"current-rank\">\n                                <h4 data-bind=\"text: name() || 'Unranked'\">Rank</h4>\n                                <div class=\"imgDiv\">\n                                    <img data-bind=\"attr: { src: imgSrcAttr(), title: description() }\" alt=\"goal-image\" src=\"/images/logo/Warriors-Guild-icon-sm.png\" />\n                                </div>\n                                <h4 data-bind=\"text: $component.UserFullName\">Warrior Name</h4>\n                            </div>\n                            <div class=\"sub-ranks\">\n                                <div>\n                                    <img title=\"Apprentice\" alt=\"Apprentice\" data-bind=\"attr: { src: percentComplete() >= 33 ? '/images/apprentice01.png' : '/images/gray01.png', alt: 'Apprentice' + (percentComplete() == 100 ? '' : ' - Not Achieved') }\" src=\"/images/gray01.png\" />\n                                    <img title=\"Journeyman\" alt=\"Journeyman\" data-bind=\"attr: { src: percentComplete() >= 66 ? '/images/journeyman01.png' : '/images/gray01.png', alt: 'Journeyman' + (percentComplete() == 100 ? '' : ' - Not Achieved') }\" src=\"/images/gray01.png\" />\n                                    <img title=\"Master\" alt=\"Master\" data-bind=\"attr: { src: percentComplete() === 100 ? '/images/master01.png' : '/images/gray01.png', alt: 'Master' + (percentComplete() == 100 ? '' : ' - Not Achieved') }\" src=\"/images/gray01.png\" />\n                                </div>\n                            </div>\n                            <!-- /ko -->\n                        </div>\n                    </div>\n                    \n                </div>"
});
//<div class="col-xs-12 col-sm-6 col-md-7 col-lg-8" >
//    <div class="row" data - bind="if: showFavoriteData" >
//        <div class="col-xs-12" >
//            <!--ko if: hasFavoriteVerse()-- >
//                <div>FAVORITE VERSE: <span data - bind="text: FavoriteVerse()" style = "padding-left: 10px" > </span></div >
//                    <!-- /ko -->
//                    < !--ko if: hasHobbies()-- >
//                        <div>HOBBIES: <span data - bind="text: Hobbies()" style = "padding-left: 10px" > </span></div >
//                            <!-- /ko -->
//                            < !--ko if: hasInterestingFact()-- >
//                                <div>INTERESTING FACT: <span data - bind="text: InterestingFact()" style = "padding-left: 10px" > </span></div >
//                                    <!-- /ko -->
//                                    < !--ko if: hasFavoriteMovie()-- >
//                                        <div>FAVORITE MOVIE: <span data - bind="text: FavoriteMovie()" style = "padding-left: 10px" > </span></div >
//                                            <!-- /ko -->
//                                            < !--ko if: !(hasFavoriteVerse() || hasHobbies() || hasInterestingFact() || hasFavoriteMovie())-- >
//                                                <div id="favoritesPlaceHolder" > </div>
//                                                    < !-- /ko -->
//                                                    < /div>
//                                                    < /div>
//                                                    < div class="row" >
//                                                        <div class="ring-cross-status clearfix" >
//                                                            <div class="col-xs-9 col-md-8" >
//                                                                <h4 class="ring-status hidden-xs visible-lg-inline-block" style = "width:140px; text-align: right;" > WARRIOR RINGS EARNED: </h4>
//                                                                    < div class="ring-status silver_ring" >
//                                                                        <completed-item - list - popup params = "{
//popupName: 'silverRingPopup',
//    warriorId: profileId,
//        observableItems: CompletedSilverRings
//                                                                            }" />
//    < /div>
//    < div class="ring-status gold_ring" >
//        <completed-item - list - popup params = "{
//popupName: 'goldRingPopup',
//    warriorId: profileId,
//        observableItems: CompletedGoldRings
//                                                                            }" />
//    < /div>
//    < div class="ring-status platinum_ring" >
//        <completed-item - list - popup params = "{
//popupName: 'platinumRingPopup',
//    warriorId: profileId,
//        observableItems: CompletedPlatinumRings
//                                                                            }" />
//    < /div>
//    < /div>
//    < div class="col-xs-3 col-md-4" >
//        <h4 class="cross-status hidden-xs visible-lg-inline-block" > CROSSES EARNED: </h4>
//            < div class="cross-status cross" >
//                <completed-item - list - popup params = "{
//popupName: 'completedCrossPopup',
//    warriorId: profileId,
//        observableItems: CompletedCrosses
//                                                                            }" />
//    < /div>
//    < /div>
//    < /div>
//    < /div>
//    < /div-->
//# sourceMappingURL=warrior.header.component.js.map