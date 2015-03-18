using System;

namespace Troupe.Common.Interfaces {
    public interface ITroupeAuthenticator : IDisposable {
        ITroupeAuthenticationToken Authenticate(string authUrl);
        void Logout(ITroupeAuthenticationToken token);

        IAsyncResult BeginAuthenticate(string url);
        ITroupeAuthenticationToken EndAuthenticate(IAsyncResult result);
    }
}