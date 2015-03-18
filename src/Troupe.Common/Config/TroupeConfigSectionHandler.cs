using System.Configuration;
using System.Xml;

namespace Troupe.Common.Config {
    public class TroupeConfigSectionHandler : IConfigurationSectionHandler {
        public object Create(object parent, object configContext, XmlNode section)
        {
            TroupeConfigSection result = new TroupeConfigSection();

            XmlAttribute attrEnvironmentName = section.Attributes["environmentName"];
            if (attrEnvironmentName != null)
            {
                result.EnvironmentName = attrEnvironmentName.Value;
            }

            XmlAttribute attrClientKey = section.Attributes["clientKey"];
            if (attrClientKey != null)
            {
                result.ClientKey = attrClientKey.Value;
            }

            XmlAttribute attrClientSecret = section.Attributes["clientSecret"];
            if (attrClientSecret != null)
            {
                result.ClientSecret = attrClientSecret.Value;
            }

            XmlAttribute attrOAuthLoginUrl = section.Attributes["oauthLoginUrl"];
            if (attrOAuthLoginUrl != null)
            {
                result.OAuthLoginUrl = attrOAuthLoginUrl.Value;
            }

            XmlAttribute attrOAuthTokenUrl = section.Attributes["oauthTokenUrl"];
            if (attrOAuthTokenUrl != null)
            {
                result.OAuthTokenUrl = attrOAuthTokenUrl.Value;
            }

            XmlAttribute attrBaseUrl = section.Attributes["baseUrl"];
            if (attrBaseUrl != null)
            {
                result.BaseUrl = attrBaseUrl.Value;
            }

            XmlAttribute attrFaye = section.Attributes["faye"];
            if (attrFaye != null)
            {
                result.Faye = attrFaye.Value;
            }

            XmlAttribute attrSparkle = section.Attributes["sparkle"];
            if (attrSparkle != null)
            {
                result.Sparkle = attrSparkle.Value;
            }

            return result;
        }
    }
}