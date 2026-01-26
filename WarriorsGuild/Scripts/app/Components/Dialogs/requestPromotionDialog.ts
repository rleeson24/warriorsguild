namespace WarriorsGuild {
    export class RequestPromotionDialogViewModel {
        rank: KnockoutObservable<WarriorsGuild.ObservableRank> = ko.observable<WarriorsGuild.ObservableRank>(null);
        onSuccess: Function;
        isLoaded: KnockoutObservable<boolean> = ko.observable<boolean>(false);
        RequestPromotionButtonText: KnockoutComputed<"" | "Master" | "Journeyman" | "Apprentice">;
        SubmitForPromotionButtonText: KnockoutComputed<"" | "Submit for Promotion" | "Request Round Table">;
        constructor(params) {
            this.rank = params.rank;
            this.onSuccess = params.onSuccess
            this.isLoaded(!this.isLoaded());
            this.RequestPromotionButtonText = ko.computed(() => {
                if (!this.rank()) return '';
                this.isLoaded();
                return this.rank().completedPercent() == 100 ? 'Master'
                    : this.rank().completedPercent() >= 66 ? 'Journeyman'
                        : this.rank().completedPercent() >= 33 ? 'Apprentice'
                            : '';
            })

            this.SubmitForPromotionButtonText = ko.computed(() => {
                if (!this.rank()) return '';
                this.isLoaded();
                return this.rank().completedPercent() < 100
                    ? 'Submit for Promotion'
                    : 'Request Round Table';
            })
        }

        submitForPromotion = (): void => {
            var self = this;
            $('#requestPromotionPopup').modal('hide');
            WarriorsGuild.serviceBase.post({
                url: `/api/rankstatus/SubmitForApproval/${self.rank().id()}`,
                contentType: "application/json; charset=utf-8",
                success: function () {
                    BootstrapAlert.success({
                        title: "Action Success!",
                        message: "Rank submitted for promotion"
                    });
                    self.onSuccess();
                },
                error: function (err: JQueryXHR) {
                    BootstrapAlert.alert({
                        title: "Action Failed!",
                        message: "Could not submit this Rank for promotion"
                    });
                }
            });
        }        
    }
}

ko.components.register('request-promotion-dialog', {
    viewModel: WarriorsGuild.RequestPromotionDialogViewModel,
    template: `<div class="modal fade" id="requestPromotionPopup" tabindex="-1" role="dialog" aria-labelledby="modalLabel" aria-hidden="true">
                    <div class="modal-dialog" role="document">
                        <div class="modal-content">
                            <div class="modal-header">
                                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                    <span aria-hidden="true">&times;</span>
                                </button>
                                <h4 class="modal-title" id="modalLabel">Request Promotion</h4>
                            </div>
                            <div class="modal-body">
                                <h1 style="width:100%;">Congratulations!</h1>
                                <h2 style="width:100%;">You have completed enough of this Rank to level up to <span data-bind="text: RequestPromotionButtonText()"></span>!</h2>
                            </div>
                            <div class="modal-footer">
                                <button class="btn btn-primary" data-bind="click: submitForPromotion, text: SubmitForPromotionButtonText()" data-dismiss="modal"></button>
                            </div>
                        </div>
                    </div>
                </div>`
});