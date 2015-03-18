namespace Troupe.Common.Interfaces {
    public interface ITroupeClientConfig {
        string ClientKey { get; set; }
        string ClientSecret { get; set; }
        string OAuthLoginUrl { get; set; }
        string OAuthTokenUrl { get; set; }
        string BaseUrl { get; set; }
        string Faye { get; set; }
        string Sparkle { get; set; }
        string EnvironmentName { get; set; }
    }
}