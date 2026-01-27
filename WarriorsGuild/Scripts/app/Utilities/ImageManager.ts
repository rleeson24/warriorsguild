namespace WarriorsGuild.Utilities {
	export class ImageManager {
		static registerAjaxForm = ( parentId: string, uploadFormId: string, imageUploadUrl: string, imgSrcUrl: string, fileClass: string, maxSize: number | null, app: WarriorsGuild.AppViewModel, successCallback: Function ) => {
			$( '#' + uploadFormId ).ajaxForm( <JQueryFormOptions><any>{
				//dataType: 'json',
				url: imageUploadUrl,
				headers: {
					'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
				},
				beforeSubmit: function ( formData: FormData, form, options ) {
					options.extraData = {};

					var allFilesWithinParameters = true;
					var totalUploadSize = 0;
					formData.forEach( ( fileData: FormDataEntryValue ) => {
						let fd = <{ type: string, value: { type: string, size: number } }><any>fileData;
						if ( fd.type === 'file' ) {
							var file = fd.value;
							if ( fileClass === 'image' ) {
								//Check the file type.
								if ( fileClass == "image" && !file.type.match( 'image.*' ) ) {
									BootstrapAlert.alert( {
										title: "Upload Failed!",
										message: "You cannot upload this file because it’s not an image."
									} );
									allFilesWithinParameters = false;
									return;
								}
							}

							totalUploadSize += file.size;
							if ( maxSize !== null && totalUploadSize >= maxSize ) {
								BootstrapAlert.alert( {
									title: "Upload Failed!",
									message: "You cannot upload this file(s) because the size exceeds the maximum limit of " + ( maxSize / 1000 ) + " KB."
								} );
								allFilesWithinParameters = false;
								return;
							}
						}
					} );
					return allFilesWithinParameters;
				},
				beforeSend: function ( xhr, s ) {
					xhr.headers = {
						Authorization: 'Bearer ' + app.dataModel.getAccessToken()
					};
				},
				success: function ( responseText: string, statusText: string, xhr, $form ) { WarriorsGuild.Utilities.ImageManager.fileUploadSuccess( responseText, parentId, imgSrcUrl, $form ); successCallback( xhr.responseJSON ); },
				error: function ( xhr, status, error ) {
					WarriorsGuild.Utilities.ImageManager.fileUploadError( uploadFormId, xhr, status, error );
				}
			} );
			switch ( fileClass ) { //image, pdf, any
				case "image":
					$( '#' + uploadFormId + " input[type='file']" ).attr( "accept", ".jpg,.png" );
					break;
				case "pdf":
					$( '#' + uploadFormId + " input[type='file']" ).attr( "accept", ".pdf" );
					break;
				case "any":
					$( '#' + uploadFormId + " input[type='file']" ).attr( "accept", "*" );
					break;
			}
			$( '#' + uploadFormId ).data( "fileClass", fileClass );
			WarriorsGuild.Utilities.ImageManager.bindFileChangeEvent( uploadFormId );
		};

		static fileUploadSuccess( response: string, parentId: string, imgSrcUrl: string, $form ) {
			if ( $form.data( "fileClass" ) === "image" ) {
				//var fileMetadata = response;
				var jImg = $form.closest('.image-upload').find('img');
				var fileNameWithExtension = imgSrcUrl.split("?")[0];
				var currentFileExtension = fileNameWithExtension.split('.').pop();
				var newFileExtension = $form[0][0].files[0].name.split('.').pop();
				var d = new Date();
				//var img = ( <HTMLScriptElement[]><any>jImg )[0];
				jImg.attr("src", fileNameWithExtension.replace(currentFileExtension, newFileExtension) + '?' + d.getTime() );
			}
			BootstrapAlert.success( {
				title: "Upload Success!",
				message: 'Your file was successfully uploaded'
			} );
		};

		static fileUploadError( uploadFormId, xhr, status, error ) {
			//$('#DocumentsUploadError').show();
			//$('#' + uploadFormId + 'Failed').show();
			BootstrapAlert.warning( {
				title: "Upload Failed!",
				message: ( !xhr.responseJSON ? 'uh oh!' : xhr.responseJSON.message )
			} );
		};

		static getFileNameFromPath( filePath ) {
			return filePath.match( /[^\/\\]+$/ )[0];
		};

		static getFileExtension( filename, fileClass: string ) {
			var allowedFileTypes = '';
			switch ( fileClass ) {
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
			var matchArray = filename.match( new RegExp( '\.(' + allowedFileTypes + ')$', 'i' ) );
			return matchArray ? matchArray[0].toLowerCase() : null;
		};

		static bindFileChangeEvent( uploadFormId ) {
			$( '#' + uploadFormId + ' input[type="file"]' ).off( 'change', WarriorsGuild.Utilities.ImageManager.onFileChange );
			$( '#' + uploadFormId + ' input[type="file"]' ).on( 'change', WarriorsGuild.Utilities.ImageManager.onFileChange );
		};

		static onFileChange( e ) {
			if ( e.type === e.originalEvent.type ) {
				var filePath = $( e.currentTarget ).val();
				var theForm = $( this ).closest( 'form' );
				var fileClass = theForm.data( "fileClass" );
				if ( filePath !== '' ) {
					var filename = WarriorsGuild.Utilities.ImageManager.getFileNameFromPath( filePath );
					var fileExt = WarriorsGuild.Utilities.ImageManager.getFileExtension( filename, fileClass );
					if ( fileExt !== null ) {
						WarriorsGuild.Utilities.ImageManager.SubmitForm( theForm );
					}
					else {
						switch ( fileClass ) {
							case "image":
								alert( 'Valid file types are: .jpg, .jpeg, .png, .bmp' );
								break;
							case "pdf":
								alert( 'File must be a .pdf' );
								break;
							case "any":
								alert( 'Valid file types are: .pdf, .jpg, .jpeg, .tif, .tiff, .png, .bmp' );
								break;
						}
						return false;
					}
				}
			}
		};


		static SubmitForm( form ) {
			form.submit();
		};
	}
}

$( "div.triggerAttachment" ).on( 'click', () => {
	$( this ).children( 'input[type="file"]' ).trigger( 'click' );
} );