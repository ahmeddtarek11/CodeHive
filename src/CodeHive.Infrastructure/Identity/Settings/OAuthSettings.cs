using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Infrastructure.Identity;

public sealed class OAuthSettings
{
    public ProviderSettings Google  { get; set; } = new();
    public ProviderSettings GitHub { get; set; } = new();
    public string FrontendCallbackUrl { get; set; } = string.Empty;

    public sealed class ProviderSettings
    {
        public string ClientId  { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
