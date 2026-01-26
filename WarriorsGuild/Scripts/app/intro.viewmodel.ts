namespace WarriorsGuild {
    export class IntroViewModel {
        app: AppViewModel;
        dataModel: {
            typedName: KnockoutObservable<string>;
            introScrolledToBottom: KnockoutObservable<boolean>;
            signEnabled: KnockoutComputed<boolean>;
            Submitting: KnockoutObservable<boolean>;
        };
        stripeControl: WarriorsGuild.WgStripe;
        constructor( app: WarriorsGuild.AppViewModel ) {
            this.app = app;
            var self = this;
            this.dataModel = {
                typedName: ko.observable<string>(),
                introScrolledToBottom: ko.observable<boolean>(),
                signEnabled: ko.pureComputed<boolean>(() => {
                    return this.dataModel.introScrolledToBottom() && this.dataModel.typedName() > '';
                }),
                Submitting: ko.observable<boolean>(false)
            };
            app.prepareAjax();

            Sammy( function () {
                this.get( '', function ( routeParams ) {
                } );
            } );
        }

        introScroll = (vm, event): void => {
            let myDiv = event.currentTarget;
            if (myDiv.offsetHeight + myDiv.scrollTop >= myDiv.scrollHeight) {
                this.dataModel.introScrolledToBottom(true);
            }
        }

        signCovenant = (): void => {
            var self = this;
            self.dataModel.Submitting(true);
            $.ajax({
                method: 'post',
                url: `/api/warrior/signcovenant`,
                contentType: "application/json; charset=utf-8",
                data: '"' + self.dataModel.typedName + '"',
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function () {
                    window.location.assign('/');
                },
                error: function (err: JQueryXHR) {
                    self.dataModel.Submitting(false);
                    BootstrapAlert.alert({
                        title: "Signing Failed!",
                        message: "Signing covenant failed" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err) 
                    });
                },
            });
        };

        confirmVideoWatched = (): void => {
            var self = this;
            (<HTMLButtonElement>document.getElementById('btnContinue')).disabled = true;
            $.ajax({
                method: 'post',
                url: `/api/guardian/confirmVideoWatched`,
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + self.app.dataModel.getAccessToken()
                },
                success: function () {
                    window.location.assign('/');
                },
                error: function (err: JQueryXHR) {
                    (<HTMLButtonElement>document.getElementById('btnContinue')).disabled = false;
                    BootstrapAlert.alert({
                        title: "Confirmation Failed!",
                        message: "Failed to confirm watching of video" + WarriorsGuild.ParseResponseErrorWithLeadingPeriod(err)
                    });
                },
            });
        };
    }
}

WarriorsGuild.app.addViewModel( {
    name: "Intro",
    bindingMemberName: "intro",
    factory: WarriorsGuild.IntroViewModel
} );