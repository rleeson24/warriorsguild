var oidcConfig = {
    userStore: new Oidc.WebStorageStateStore({ store: window.localStorage }),
    authority: window.location.origin,
    client_id: "js",
    redirect_uri: "".concat(window.location.origin, "/callback.html"),
    response_type: "id_token token",
    scope: "openid profile IdentityServerApi",
    post_logout_redirect_uri: window.location.origin,
};
var oidcManager = new Oidc.UserManager(oidcConfig);
//# sourceMappingURL=oidcSettings.js.map