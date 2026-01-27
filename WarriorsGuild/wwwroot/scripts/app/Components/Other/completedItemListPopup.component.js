var WarriorsGuild;
(function (WarriorsGuild) {
    var CompletedItemListPopupViewModel = /** @class */ (function () {
        function CompletedItemListPopupViewModel(params) {
            var _this = this;
            this.showPopup = function () {
                $.each($(".popuptext:not(#".concat(_this.popupName).concat(_this.warriorId, ")")), function (index, el) {
                    el.classList.remove('show');
                });
                var popupElement = document.getElementById(_this.popupName + _this.warriorId);
                popupElement ? popupElement.classList.toggle("show") : null;
            };
            this.closePopup = function () {
                var popupElement = document.getElementById(_this.popupName + _this.warriorId);
                popupElement.classList.toggle("show");
            };
            this.popupName = params.popupName;
            this.warriorId = params.warriorId;
            this.items = params.observableItems;
        }
        return CompletedItemListPopupViewModel;
    }());
    WarriorsGuild.CompletedItemListPopupViewModel = CompletedItemListPopupViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
ko.components.register('completed-item-list-popup', {
    viewModel: WarriorsGuild.CompletedItemListPopupViewModel,
    template: "<div class=\"popup\" data-bind=\"click: showPopup\">\n\t<h2 data-bind=\"text: items.length\"></h2>\n\t<!-- ko if: items.length > 0 -->\n\t<div style=\"max-height:300px; height: auto; overflow-y: scroll\">\n\t\t<div class=\"popuptext\" data-bind=\"attr: { 'id':popupName + warriorId, event: { mouseleave: closePopup }\">\n\t\t\t<!-- ko foreach: items -->\n\t\t\t<div>\n\t\t\t\t<img data-bind=\"attr: { src: imgSrcAttr() }\" /><span><span data-bind=\"text: name()\"></span></span>\n\t\t\t</div>\n\t\t\t<!-- /ko -->\n\t\t</div>\n\t</div>\n\t<!-- /ko -->\n</div>\n"
});
//# sourceMappingURL=completedItemListPopup.component.js.map