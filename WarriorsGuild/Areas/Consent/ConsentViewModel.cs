// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI
{
    public class ConsentViewModel : ConsentInputModel
    {
        public string ClientName { get; set; } = default!;
        public string ClientUrl { get; set; } = default!;
        public string ClientLogoUrl { get; set; } = default!;
        public bool AllowRememberConsent { get; set; }

        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; } = default!;
        public IEnumerable<ScopeViewModel> ApiScopes { get; set; } = default!;
    }
}
