namespace WarriorsGuild {
	export class CompletedItemListPopupViewModel {
		popupName: string;
		warriorId: string;
		items: KnockoutObservableArray<MinimumGoalDetail>;

		constructor(params) {
			this.popupName = params.popupName
			this.warriorId = params.warriorId;
			this.items = params.observableItems;
        }

		showPopup = () => {
			$.each($(`.popuptext:not(#${this.popupName}${this.warriorId})`), (index, el) => {
				el.classList.remove('show');
			});
			const popupElement = document.getElementById(this.popupName + this.warriorId);
			popupElement ? popupElement.classList.toggle(`show`) : null;
		};

		closePopup = () => {
			const popupElement = document.getElementById(this.popupName + this.warriorId);
			popupElement.classList.toggle(`show`);
		};
	}
}


ko.components.register('completed-item-list-popup', {
	viewModel: WarriorsGuild.CompletedItemListPopupViewModel,
	template:
`<div class="popup" data-bind="click: showPopup">
	<h2 data-bind="text: items.length"></h2>
	<!-- ko if: items.length > 0 -->
	<div style="max-height:300px; height: auto; overflow-y: scroll">
		<div class="popuptext" data-bind="attr: { 'id':popupName + warriorId, event: { mouseleave: closePopup }">
			<!-- ko foreach: items -->
			<div>
				<img data-bind="attr: { src: imgSrcAttr() }" /><span><span data-bind="text: name()"></span></span>
			</div>
			<!-- /ko -->
		</div>
	</div>
	<!-- /ko -->
</div>
`
});