var oidcConfig = {
    userStore: new Oidc.WebStorageStateStore({ store: window.localStorage }),
    authority: window.location.origin,
    client_id: "js",
    redirect_uri: "".concat(window.location.origin, "/callback.html"),
    response_type: "code",
    scope: "openid profile IdentityServerApi offline_access",
    post_logout_redirect_uri: window.location.origin,
};
var oidcManager = new Oidc.UserManager(oidcConfig);
//# sourceMappingURL=oidcSettings.js.map