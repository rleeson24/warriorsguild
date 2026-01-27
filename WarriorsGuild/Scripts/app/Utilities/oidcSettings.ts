let oidcConfig: Oidc.UserManagerSettings = {
	userStore: new Oidc.WebStorageStateStore({ store: window.localStorage }),
	authority: window.location.origin,
	client_id: "js",
	redirect_uri: `${window.location.origin}/callback.html`,
	response_type: "id_token token",
	scope: "openid profile IdentityServerApi",
	post_logout_redirect_uri: window.location.origin,
};

let oidcManager = new Oidc.UserManager(oidcConfig);