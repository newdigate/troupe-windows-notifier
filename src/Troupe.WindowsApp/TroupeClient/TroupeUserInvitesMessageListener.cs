using System;
using System.Collections.Generic;
using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using log4net;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeUserInvitesMessageListener : IMessageListener {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TroupeUserSessionMessageListener));

        private readonly Action<ITroupeInvite> _inviteReceived;
        private readonly Action<ITroupeInvite> _inviteRemoved;
        private readonly TroupeInviteExtractor _inviteDeserializer;

        public TroupeUserInvitesMessageListener(Action<ITroupeInvite> inviteReceived, Action<ITroupeInvite> inviteRemoved)
        {
            _inviteReceived = inviteReceived;
            _inviteRemoved = inviteRemoved;
            _inviteDeserializer = new TroupeInviteExtractor();
        }

        public void onMessage(IClientSessionChannel channel, IMessage message) {
            Dictionary<string, object> data = (Dictionary<string, object>)message["data"];
            string operation = (string)data["operation"];
            switch (operation)
            {
                case "create":
                    Dictionary<string, object> model = (Dictionary<string, object>)data["model"];
                    ITroupeInvite invite = _inviteDeserializer.CreateTroupeInvite(model);
                    if (_inviteReceived != null)
                        _inviteReceived(invite);
                    break;

                case "remove":
                    Dictionary<string, object> modelToRemove = (Dictionary<string, object>)data["model"];
                    ITroupeInvite inviteToRemove = _inviteDeserializer.CreateTroupeInvite(modelToRemove);
                    if (_inviteRemoved != null)
                        _inviteRemoved(inviteToRemove);
                    break;
                default:
                    logger.InfoFormat("Ignored operation {0}", operation);
                    break;
            }
        }
    }
}