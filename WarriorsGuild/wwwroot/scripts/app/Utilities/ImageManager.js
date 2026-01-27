var _this = this;
var WarriorsGuild;
(function (WarriorsGuild) {
    var Utilities;
    (function (Utilities) {
        var ImageManager = /** @class */ (function () {
            function ImageManager() {
            }
            ImageManager.fileUploadSuccess = function (response, parentId, imgSrcUrl, $form) {
                if ($form.data("fileClass") === "image") {
                    //var fileMetadata = response;
                    var jImg = $form.closest('.image-upload').find('img');
                    var fileNameWithExtension = imgSrcUrl.split("?")[0];
                    var currentFileExtension = fileNameWithExtension.split('.').pop();
                    var newFileExtension = $form[0][0].files[0].name.split('.').pop();
                    var d = new Date();
                    //var img = ( <HTMLScriptElement[]><any>jImg )[0];
                    jImg.attr("src", fileNameWithExtension.replace(currentFileExtension, newFileExtension) + '?' + d.getTime());
                }
                BootstrapAlert.success({
                    title: "Upload Success!",
                    message: 'Your file was successfully uploaded'
                });
            };
            ;
            ImageManager.fileUploadError = function (uploadFormId, xhr, status, error) {
                //$('#DocumentsUploadError').show();
                //$('#' + uploadFormId + 'Failed').show();
                BootstrapAlert.warning({
                    title: "Upload Failed!",
                    message: (!xhr.responseJSON ? 'uh oh!' : xhr.responseJSON.message)
                });
            };
            ;
            ImageManager.getFileNameFromPath = function (filePath) {
                return filePath.match(/[^\/\\]+$/)[0];
            };
            ;
            ImageManager.getFileExtension = function (filename, fileClass) {
                var allowedFileTypes = '';
                switch (fileClass) {
                    case "image":
                        allowedFileTypes = 'jpg|jpeg|gif|png|bmp';
                        break;
                    case "pdf":
                        allowedFileTypes = 'pdf';
                        break;
                    case "any":
                        allowedFileTypes = 'jpg|jpeg|tif|tiff|gif|png|pdf|bmp';
                        break;
                }
                var matchArray = filename.match(new RegExp('\.(' + allowedFileTypes + ')$', 'i'));
                return matchArray ? matchArray[0].toLowerCase() : null;
            };
            ;
            ImageManager.bindFileChangeEvent = function (uploadFormId) {
                $('#' + uploadFormId + ' input[type="file"]').off('change', WarriorsGuild.Utilities.ImageManager.onFileChange);
                $('#' + uploadFormId + ' input[type="file"]').on('change', WarriorsGuild.Utilities.ImageManager.onFileChange);
            };
            ;
            ImageManager.onFileChange = function (e) {
                if (e.type === e.originalEvent.type) {
                    var filePath = $(e.currentTarget).val();
                    var theForm = $(this).closest('form');
                    var fileClass = theForm.data("fileClass");
                    if (filePath !== '') {
                        var filename = WarriorsGuild.Utilities.ImageManager.getFileNameFromPath(filePath);
                        var fileExt = WarriorsGuild.Utilities.ImageManager.getFileExtension(filename, fileClass);
                        if (fileExt !== null) {
                            WarriorsGuild.Utilities.ImageManager.SubmitForm(theForm);
                        }
                        else {
                            switch (fileClass) {
                                case "image":
                                    alert('Valid file types are: .jpg, .jpeg, .png, .bmp');
                                    break;
                                case "pdf":
                                    alert('File must be a .pdf');
                                    break;
                                case "any":
                                    alert('Valid file types are: .pdf, .jpg, .jpeg, .tif, .tiff, .png, .bmp');
                                    break;
                            }
                            return false;
                        }
                    }
                }
            };
            ;
            ImageManager.SubmitForm = function (form) {
                form.submit();
            };
            ;
            ImageManager.registerAjaxForm = function (parentId, uploadFormId, imageUploadUrl, imgSrcUrl, fileClass, maxSize, app, successCallback) {
                $('#' + uploadFormId).ajaxForm({
                    //dataType: 'json',
                    url: imageUploadUrl,
                    headers: {
                        'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                    },
                    beforeSubmit: function (formData, form, options) {
                        options.extraData = {};
                        var allFilesWithinParameters = true;
                        var totalUploadSize = 0;
                        formData.forEach(function (fileData) {
                            var fd = fileData;
                            if (fd.type === 'file') {
                                var file = fd.value;
                                if (fileClass === 'image') {
                                    //Check the file type.
                                    if (fileClass == "image" && !file.type.match('image.*')) {
                                        BootstrapAlert.alert({
                                            title: "Upload Failed!",
                                            message: "You cannot upload this file because it’s not an image."
                                        });
                                        allFilesWithinParameters = false;
                                        return;
                                    }
                                }
                                totalUploadSize += file.size;
                                if (maxSize !== null && totalUploadSize >= maxSize) {
                                    BootstrapAlert.alert({
                                        title: "Upload Failed!",
                                        message: "You cannot upload this file(s) because the size exceeds the maximum limit of " + (maxSize / 1000) + " KB."
                                    });
                                    allFilesWithinParameters = false;
                                    return;
                                }
                            }
                        });
                        return allFilesWithinParameters;
                    },
                    beforeSend: function (xhr, s) {
                        xhr.headers = {
                            Authorization: 'Bearer ' + app.dataModel.getAccessToken()
                        };
                    },
                    success: function (responseText, statusText, xhr, $form) { WarriorsGuild.Utilities.ImageManager.fileUploadSuccess(responseText, parentId, imgSrcUrl, $form); successCallback(xhr.responseJSON); },
                    error: function (xhr, status, error) {
                        WarriorsGuild.Utilities.ImageManager.fileUploadError(uploadFormId, xhr, status, error);
                    }
                });
                switch (fileClass) { //image, pdf, any
                    case "image":
                        $('#' + uploadFormId + " input[type='file']").attr("accept", ".jpg,.png");
                        break;
                    case "pdf":
                        $('#' + uploadFormId + " input[type='file']").attr("accept", ".pdf");
                        break;
                    case "any":
                        $('#' + uploadFormId + " input[type='file']").attr("accept", "*");
                        break;
                }
                $('#' + uploadFormId).data("fileClass", fileClass);
                WarriorsGuild.Utilities.ImageManager.bindFileChangeEvent(uploadFormId);
            };
            return ImageManager;
        }());
        Utilities.ImageManager = ImageManager;
    })(Utilities = WarriorsGuild.Utilities || (WarriorsGuild.Utilities = {}));
})(WarriorsGuild || (WarriorsGuild = {}));
$("div.triggerAttachment").on('click', function () {
    $(_this).children('input[type="file"]').trigger('click');
});
//# sourceMappingURL=ImageManager.js.map