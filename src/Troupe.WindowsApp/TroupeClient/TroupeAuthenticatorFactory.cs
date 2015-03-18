using System.Threading;
using System.Windows.Forms;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeAuthenticatorFactory : IFactory<ITroupeAuthenticator> {
        private readonly Form _parentForm;
        private readonly ITroupeAuthTokenHandler _authTokenHandler;
        private readonly ITroupeClientConfig _troupeClientConfig;
        private readonly ManualResetEvent _signalComplete;
        private readonly ISessionClientDataStore _sessionClientDataStore;
        public TroupeAuthenticatorFactory(Form parentForm, ITroupeAuthTokenHandler authTokenHandler, ITroupeClientConfig troupeClientConfig, ManualResetEvent signalComplete, ISessionClientDataStore sessionClientDataStore) {
            _parentForm = parentForm;
            _sessionClientDataStore = sessionClientDataStore;
            _signalComplete = signalComplete;
            _troupeClientConfig = troupeClientConfig;
            _authTokenHandler = authTokenHandler;
        }

        public ITroupeAuthenticator Create() {
            return new TroupeAuthenticator(parentForm, authTokenHandler, troupeClientConfig, _signalComplete, _sessionClientDataStore);
        }

        protected ITroupeClientConfig troupeClientConfig {
            get {
                return _troupeClientConfig;
            }
        }

        protected ITroupeAuthTokenHandler authTokenHandler {
            get {
                return _authTokenHandler;
            }
        }

        protected Form parentForm {
            get {
                return _parentForm;
            }
        }
    }
}