using System;
using Cometd.Bayeux;
using Cometd.Bayeux.Client;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeUserTroupesSessionMessageListener : IMessageListener {
        private readonly Action<object> _onTroupeUserTroupesMessageReceived;

        public TroupeUserTroupesSessionMessageListener(Action<object> onTroupeUserTroupesMessageReceived) {
            _onTroupeUserTroupesMessageReceived = onTroupeUserTroupesMessageReceived;
        }

        public void onMessage(IClientSessionChannel channel, IMessage message) {
            if (_onTroupeUserTroupesMessageReceived  != null) {
                _onTroupeUserTroupesMessageReceived(message);
            }
        }
    }
}