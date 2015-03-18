using System;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    [Serializable]
    public class TroupeAuthenticationToken : ITroupeAuthenticationToken {
        public string SessionToken { get; set; }
        public string ClientData { get; set; }
    }
}