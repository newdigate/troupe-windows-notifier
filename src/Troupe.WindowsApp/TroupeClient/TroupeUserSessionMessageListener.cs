using System;
using System.Collections.Generic;
using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using log4net;

namespace Troupe.WindowsApp.TroupeClient
{
    public class TroupeUserSessionMessageListener : IMessageListener {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TroupeUserSessionMessageListener));
        private readonly Action<string, string, string> _onUserNotification;
        private readonly Action<string, int> _onUserUnreadTroupeMessages;

        public TroupeUserSessionMessageListener(Action<string, string, string> onUserNotification, Action<string, int> onUserUnreadTroupeMessages) {
            _onUserNotification = onUserNotification;
            _onUserUnreadTroupeMessages = onUserUnreadTroupeMessages;
        }

        public void onMessage(IClientSessionChannel channel, IMessage message) {
            if (message.ContainsKey("data")) {
                Dictionary<string,object> data = (Dictionary<string,object>)message["data"];
                if (data.ContainsKey("notification")) {
                    
                    string notificationType = (string)data["notification"];
                    switch (notificationType) {
                        case "troupe_unread" :
                            int numUnreadMessages =  (int)data["totalUnreadItems"];
                            string troupeId = (string) data["troupeId"];
                            logger.InfoFormat("numUnreadMessages: {0}={1}", troupeId, numUnreadMessages);
                            if (_onUserUnreadTroupeMessages != null) {
                                _onUserUnreadTroupeMessages(troupeId, numUnreadMessages);
                            }
                            break;

                        case "user_notification" :
                            string title = (string)data["title"];
                            string messageText = (string)data["text"];
                            string link = (string) data["link"];
                            logger.InfoFormat("user_notification {0} {1} {2}", title, messageText, link);
                            if (_onUserNotification != null) {
                                _onUserNotification(link, title, messageText);
                            }
                            break;

                        default:
                            logger.InfoFormat("notification type ignored: {0}", notificationType);
                            break;
                    } 

                       
                }
                
            }
        }
    }
}
