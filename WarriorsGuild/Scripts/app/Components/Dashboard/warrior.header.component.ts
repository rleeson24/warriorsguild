namespace WarriorsGuild {
    export class WarriorHeaderViewModel {
        rank: KnockoutObservable<ObservableRank>
        hasAvatar: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        username: KnockoutObservable<string> = ko.observable<string>('');
        userFullName: KnockoutObservable<string> = ko.observable<string>('');
        showFavoriteData: boolean = false;
        crossService: CrossService;

        favoriteVerse: KnockoutObservable<string> = ko.observable<string>('');
        hobbies: KnockoutObservable<string> = ko.observable<string>('');
        interestingFact: KnockoutObservable<string> = ko.observable<string>('');
        favoriteMovie: KnockoutObservable<string> = ko.observable<string>('');
        hasFavoriteVerse: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => this.favoriteVerse() && this.favoriteVerse().length > 0);
        hasHobbies: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => this.hobbies() && this.hobbies().length > 0);
        hasInterestingFact: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => this.interestingFact() && this.interestingFact().length > 0);
        hasFavoriteMovie: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => this.favoriteMovie() && this.favoriteMovie().length > 0);
        CompletedCrosses: KnockoutObservableArray<ObservableCross> = ko.observableArray<ObservableCross>([]);
        CompletedSilverRings: KnockoutObservableArray<ObservableRing> = ko.observableArray<ObservableRing>([]);
        CompletedGoldRings: KnockoutObservableArray<ObservableRing> = ko.observableArray<ObservableRing>([]);
        CompletedPlatinumRings: KnockoutObservableArray<ObservableRing> = ko.observableArray<ObservableRing>([]);
        profileId: string;
        profileService: ProfileService;

        avatarPath: KnockoutComputed<string> = ko.pureComputed<string>(() => {
            return '/api/Profile/' + this.profileId + '/Avatar'
        });
        ringService: RingService;

        constructor(params: any) {
            this.crossService = new CrossService();
            this.showFavoriteData = params.showFavoriteData;
            this.profileId = params.userId;
            this.rank = params.rankDetail;
            this.ringService = new RingService();
            this.profileService = new ProfileService();
            this.profileService.retrieveProfileData(this.profileId, this.loadProfileData);
            this.getCompletedRings();
            this.getCompletedCrosses();
        }

        loadProfileData = (warriorDashboardViewModel: Profile) => {
            this.userFullName(warriorDashboardViewModel.userFullName);
            this.favoriteVerse(warriorDashboardViewModel.favoriteVerse);
            this.hobbies(warriorDashboardViewModel.hobbies);
            this.interestingFact(warriorDashboardViewModel.interestingFact);
            this.favoriteMovie(warriorDashboardViewModel.favoriteMovie);
        }

        getCompletedRings = () => {
            let self = this;

            this.ringService.getCompletedRings(this.profileId,
                (data: Ring[]) => {
                    var silver = 'Silver', gold = 'Gold', platinum = 'Platinum';
                    $.each(data.sort((a, b) => a.name.localeCompare(b.name)), function (i, v) {
                        var oRing = <ObservableRing><any>ko.mapping.fromJS(v, self.koMapperConfiguration, <KnockoutObservableType<Ring>><any>new ObservableRing());
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
                },
                (err: JQueryXHR) => {
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
        }

        getCompletedCrosses = () => {
            let self = this;
            this.crossService.getCompletedCrossesForUser(this.profileId,
                (data: Cross[]) => {
                    $.each(data.sort((a, b) => a.name.localeCompare(b.name)), function (i, v) {
                        var oCross = <ObservableCross><any>ko.mapping.fromJS(v, self.koMapperConfiguration, <KnockoutObservableType<Cross>><any>new ObservableCross());
                        self.CompletedCrosses.push(oCross);
                    });
                },
                (err: JQueryXHR) => {
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
        }

        private koMapperConfiguration = (() => {
            var self = this;
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
                        return ko.mapping.fromJS(options.data, {}, new ObservableRankRequirement());
                    }
                }
            };
        })();
    }
}


ko.components.register('warior-header', {
    viewModel: WarriorsGuild.WarriorHeaderViewModel,
    template: `<div class="row">
                    <div class="col-md-offset-3 col-md-5 col-lg-4">
                        <div class="photo-and-rank">
                            <div class="user-photo">
                                <!-- ko if: $component.hasAvatar() -->
                                <img data-bind="attr: { src: $component.avatarPath() }" style="margin-top: -50px; width: 160px; height: 160px; border-radius: 50%">
                                <!-- /ko -->
                                <!-- ko if: !$component.hasAvatar() -->
                                <span data-bind="text: $component.username"></span>
                                <!-- /ko -->
                            </div>
                            <!-- ko with: rank() -->
                            <div class="current-rank">
                                <h4 data-bind="text: name() || 'Unranked'">Rank</h4>
                                <div class="imgDiv">
                                    <img data-bind="attr: { src: imgSrcAttr(), title: description() }" alt="goal-image" src="/images/logo/Warriors-Guild-icon-sm.png" />
                                </div>
                                <h4 data-bind="text: $component.UserFullName">Warrior Name</h4>
                            </div>
                            <div class="sub-ranks">
                                <div>
                                    <img title="Apprentice" alt="Apprentice" data-bind="attr: { src: percentComplete() >= 33 ? '/images/apprentice01.png' : '/images/gray01.png', alt: 'Apprentice' + (percentComplete() == 100 ? '' : ' - Not Achieved') }" src="/images/gray01.png" />
                                    <img title="Journeyman" alt="Journeyman" data-bind="attr: { src: percentComplete() >= 66 ? '/images/journeyman01.png' : '/images/gray01.png', alt: 'Journeyman' + (percentComplete() == 100 ? '' : ' - Not Achieved') }" src="/images/gray01.png" />
                                    <img title="Master" alt="Master" data-bind="attr: { src: percentComplete() === 100 ? '/images/master01.png' : '/images/gray01.png', alt: 'Master' + (percentComplete() == 100 ? '' : ' - Not Achieved') }" src="/images/gray01.png" />
                                </div>
                            </div>
                            <!-- /ko -->
                        </div>
                    </div>
                    
                </div>`
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