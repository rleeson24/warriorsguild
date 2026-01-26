namespace WarriorsGuild {
	export class CustomCheckbox {
		text: string;
		id: string;
		checked: KnockoutObservable<boolean>;
		data: object[];

		constructor(params) {
			this.text = params.text
			this.id = params.id;
			this.checked = params.checked;
			this.data = params.data;
        }
	}
}


ko.components.register('custom-checkbox', {
	viewModel: WarriorsGuild.CustomCheckbox,
	template:
`<div class="check">
	<div class="squaredThree">
		<input type="checkbox" data-bind="attr: { id: id },
							checked: checked" />
		<label data-bind="attr: { for: id }"></label>
	</div>
</div>
<label data-bind="attr: { for: id }, text: text"></label>
<!-- ko template: { nodes: $componentTemplateNodes, data: data } --><!-- /ko -->
`
});