var WarriorsGuild;
(function (WarriorsGuild) {
    var BlogViewModel = /** @class */ (function () {
        function BlogViewModel(app, dataModel) {
            var self = this;
            this.dataModel = {
                BlogEntries: ko.observableArray([]),
                BlogEntry: ko.observable(),
                RetrievingPlans: ko.observable(false)
            };
            self.retrievePlansFailure = ko.observable(false);
            this.dataModel.BlogEntries = ko.observableArray([]);
            Sammy(function () {
                this.get('/Blog/:blogId', function () {
                    this.app.runRoute('get', '/blog/' + this.params['blogId']);
                });
                this.get('/blog/:blogId', function () {
                    self.dataModel.BlogEntries.removeAll();
                    //$.ajax( {
                    //	method: 'get',
                    //	url: 'https://public-api.wordpress.com/rest/v1.1/sites/www.warriorsguild.com/posts/' + this.params['blogId'],
                    //	contentType: "application/json; charset=utf-8",
                    //	success: function ( data ) {
                    //		self.dataModel.BlogEntry( data );
                    //	}
                    //} );
                    //self.dataModel.RetrievingPlans( true );
                    //self.retrievePlansFailure( false );
                });
                this.get('#blog', function () {
                    self.dataModel.BlogEntry(null);
                    //$.ajax( {
                    //	method: 'get',
                    //	url: 'https://public-api.wordpress.com/rest/v1.1/sites/www.warriorsguild.com/posts/',
                    //	contentType: "application/json; charset=utf-8",
                    //	success: function ( data ) {
                    //		$.each( data.posts, function ( index, data ) {
                    //			self.dataModel.BlogEntries.push( data );
                    //		} );
                    //	}
                    //} );
                    //self.dataModel.RetrievingPlans( true );
                    //self.retrievePlansFailure( false );
                });
                this.get('/Blog', function () { this.app.runRoute('get', '/blog'); });
                this.get('/blog', function () { this.app.runRoute('get', '#blog'); });
            });
        }
        return BlogViewModel;
    }());
    WarriorsGuild.BlogViewModel = BlogViewModel;
})(WarriorsGuild || (WarriorsGuild = {}));
WarriorsGuild.app.addViewModel({
    name: "Blog",
    bindingMemberName: "blog",
    factory: WarriorsGuild.BlogViewModel,
    allowUnauthorized: true
});
//# sourceMappingURL=blog.viewmodel.js.map