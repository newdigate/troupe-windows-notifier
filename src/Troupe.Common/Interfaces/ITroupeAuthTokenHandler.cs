using System;

namespace Troupe.Common.Interfaces {
    public class TokenEventArgs : EventArgs {
        public string Token { get; set; }
    }
    public interface ITroupeAuthTokenHandler {
        event EventHandler<TokenEventArgs> OnNewAuthTokenReceived;
        event EventHandler<AuthFailedEventArgs> OnAuthenticationFailed;
        void InvokeNewAuthTokenEventHandler(string s);
        void InvokeAuthFailedEventHandler(string message);
    }

    public class AuthFailedEventArgs : EventArgs {
        public string Message { get; set; }

        public AuthFailedEventArgs(string message) {
            Message = message;
        }
    }
}