using System;
using Troupe.Common.Interfaces;

namespace Troupe.Common.Config
{
    [Serializable]
    public class TroupeConfigSection : ITroupeClientConfig
    {
        public string ClientKey { get; set; }

        public string ClientSecret { get; set; }

        public string OAuthLoginUrl { get; set; }

        public string OAuthTokenUrl { get; set; }

        public string BaseUrl { get; set; }

        public string Faye { get; set; }

        public string Sparkle { get; set; }

        public string EnvironmentName { get; set; }
    }
}
