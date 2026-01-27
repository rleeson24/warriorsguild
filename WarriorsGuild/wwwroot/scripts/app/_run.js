$(function () {
    var wgApp = WarriorsGuild.app;
    wgApp.initialize();
    ko.bindingHandlers.imageUploader = {
        init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            // This will be called when the binding is first applied to an element
            // Set up any initial state, event handlers, etc. here
            //console.log('Initializing widget with ' + ko.toJSON(allBindings()['imageUploder']));
            var values = valueAccessor();
            var modelId = 'fileUploadForm' + values.key;
            element.id = modelId;
            var parentId = null;
            if (typeof viewModel.id === 'function') {
                parentId = viewModel.id();
            }
            else {
                parentId = viewModel.id;
            }
            var imgSrcAttr = null;
            if (values.fileClass === "image") {
                if (viewModel.imgSrcAttr) {
                    imgSrcAttr = viewModel.imgSrcAttr();
                }
                else if (viewModel.AvatarSrc) {
                    imgSrcAttr = viewModel.AvatarSrc();
                }
            }
            WarriorsGuild.Utilities.ImageManager.registerAjaxForm(parentId, modelId, values.postUrl, imgSrcAttr, values.fileClass, values.maxSize, wgApp, values.successCallBack);
        },
        update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            // This will be called when the binding is first applied to an element
            // Set up any initial state, event handlers, etc. here
            //console.log('Initializing widget with ' + ko.toJSON(allBindings()['imageUploder']));
            var values = valueAccessor();
            var modelId = 'fileUploadForm' + values.key;
            element.id = modelId;
            var parentId = null;
            if (typeof viewModel.id === 'function') {
                parentId = viewModel.id();
            }
            else {
                parentId = viewModel.id;
            }
            var imgSrcAttr = null;
            if (values.fileClass === "image") {
                if (viewModel.imgSrcAttr) {
                    imgSrcAttr = viewModel.imgSrcAttr();
                }
                else if (viewModel.AvatarSrc) {
                    imgSrcAttr = viewModel.AvatarSrc();
                }
            }
            WarriorsGuild.Utilities.ImageManager.registerAjaxForm(parentId, modelId, values.postUrl, imgSrcAttr, values.fileClass, values.maxSize, wgApp, values.successCallBack);
        }
    };
    $('.nav').find('li a[href="' + window.location.pathname + '"]').parents('li').addClass('active');
    $('.nav li').not('.dropdown').click(function () { $('.navbar-collapse').toggle(); });
    // Activate Knockout
    ko.validation.init({ grouping: { observable: false } });
    ko.applyBindings(wgApp, document.body);
});
//# sourceMappingURL=_run.js.map