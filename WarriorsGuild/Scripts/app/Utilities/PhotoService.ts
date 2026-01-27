namespace WarriorsGuild {
    export class PhotosService {
        private httpService: any;
        private _localStorageService: any;
        public getPhotos: Function;
        public deletePhoto: Function;
        //public savePhoto: Function;

        constructor($http: any, $q: any) {
            var thisController = this;
            var accessToken = null;

            this.getPhotos = function (profileId) {
                var deferred = $q.defer();

                return $http.get('/api/Photos/GetPhotos/' + profileId, {
                    headers: {
                        'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
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

            this.deletePhoto = function (id: number) {
                var deferred = $q.defer();

                return $http.delete('/api/Photos/Delete/' + id, {
                    headers: {
                        'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
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
    }
}