var WarriorsGuild;
(function (WarriorsGuild) {
    var PhotosService = /** @class */ (function () {
        //public savePhoto: Function;
        function PhotosService($http, $q) {
            var thisController = this;
            var accessToken = null;
            this.getPhotos = function (profileId) {
                var deferred = $q.defer();
                return $http.get('/api/Photos/GetPhotos/' + profileId, {
                    headers: {
                        'Authorization': 'Bearer ' + WarriorsGuild.app.dataModel.getAccessToken()
                    }
                }).success(function (response) {
                    deferred.resolve(response);
                }).error(function (err, status) {
                    deferred.reject(err);
                });
                //return deferred.promise;
            };
            //this.savePhoto = function (id: number, model: QrProfileModel) {
            //    return $http.put('/api/QrProfiles/' + id, model, {
            //        headers: authService.getHeaders()
            //    }).then(function (response) {
            //        return response.data;
            //    },
            //        function (httpError) {
            //            // translate the error
            //            throw httpError.status + " : " +
            //            httpError.data;
            //        });
            //};
            this.deletePhoto = function (id) {
                var deferred = $q.defer();
                return $http.delete('/api/Photos/Delete/' + id, {
                    headers: {
                        'Authorization': 'Bearer ' + WarriorsGuild.app.dataModel.getAccessToken()
                    },
                }).success(function (response) {
                    deferred.resolve(response);
                }).error(function (err, status) {
                    deferred.reject(err);
                });
                //return deferred.promise;
            };
            return this;
        }
        return PhotosService;
    }());
    WarriorsGuild.PhotosService = PhotosService;
})(WarriorsGuild || (WarriorsGuild = {}));
//# sourceMappingURL=PhotoService.js.map