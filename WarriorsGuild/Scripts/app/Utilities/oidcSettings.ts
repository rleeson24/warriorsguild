let oidcConfig: Oidc.UserManagerSettings = {
	userStore: new Oidc.WebStorageStateStore({ store: window.localStorage }),
	authority: window.location.origin,
	client_id: "js",
	redirect_uri: `${window.location.origin}/callback.html`,
	response_type: "code",
	scope: "openid profile IdentityServerApi offline_access",
	post_logout_redirect_uri: window.location.origin,
};

let oidcManager = new Oidc.UserManager(oidcConfig);