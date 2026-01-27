var WarriorsGuild;
(function (WarriorsGuild) {
    var CustomCheckbox = /** @class */ (function () {
        function CustomCheckbox(params) {
            this.text = params.text;
            this.id = params.id;
            this.checked = params.checked;
            this.data = params.data;
        }
        return CustomCheckbox;
    }());
    WarriorsGuild.CustomCheckbox = CustomCheckbox;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('custom-checkbox', {
    viewModel: WarriorsGuild.CustomCheckbox,
    template: "<div class=\"check\">\n\t<div class=\"squaredThree\">\n\t\t<input type=\"checkbox\" data-bind=\"attr: { id: id },\n\t\t\t\t\t\t\tchecked: checked\" />\n\t\t<label data-bind=\"attr: { for: id }\"></label>\n\t</div>\n</div>\n<label data-bind=\"attr: { for: id }, text: text\"></label>\n<!-- ko template: { nodes: $componentTemplateNodes, data: data } --><!-- /ko -->\n"
});
//# sourceMappingURL=customCheckbox.component.js.map