using System;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.Helpers {
    public class TroupeAuthTokenHandler : ITroupeAuthTokenHandler
    {
        public event EventHandler<TokenEventArgs> OnNewAuthTokenReceived;
        public event EventHandler<AuthFailedEventArgs> OnAuthenticationFailed;

        public void InvokeNewAuthTokenEventHandler(string s) {
            if (OnNewAuthTokenReceived != null)
                OnNewAuthTokenReceived.Invoke(this, new TokenEventArgs() {Token = s});
        }

        public void InvokeAuthFailedEventHandler(string message) {
            if (OnAuthenticationFailed != null)
                OnAuthenticationFailed.Invoke(this, new AuthFailedEventArgs(message) );
        }
    }
}