namespace Troupe.Common.Interfaces {
    public interface ITroupeAuthenticationToken {
        string SessionToken { get; set; }
        string ClientData { get; set; }
    }
}