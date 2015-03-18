using System;
using System.Collections.Generic;
using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using log4net;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeExtension : IExtension {
        private static readonly ILog logger = LogManager.GetLogger(typeof (TroupeExtension));

        private readonly Action<IDictionary<string, object>> _onReceivedMetaData;
        private readonly Action<Exception> _onException;
        private readonly Action<object> _onSessionTopicDataReceived; 

        public TroupeExtension(Action<IDictionary<string, object>> onReceivedMetaData, Action<Exception> onException, Action<object> onSessionTopicDataReceived)
        {
            _onReceivedMetaData = onReceivedMetaData;
            _onSessionTopicDataReceived = onSessionTopicDataReceived;
            _onException = onException;
        }

        public bool rcv(IClientSession session, IMutableMessage message) {
            //try {
            //    logger.Debug("rcv: " + message.JSON);
            //}
            //catch (Exception exc) {
            //    logger.Error(exc);
            //}

            if (message.ContainsKey("data")) {
                if (_onSessionTopicDataReceived != null)
                    _onSessionTopicDataReceived((IDictionary<string, object>)message["data"]);
            }
            return true;
        }

        public bool rcvMeta(IClientSession session, IMutableMessage message) {
            //logger.Debug("rcvMeta: " + message.JSON);
            if (message.ContainsKey("exception")) {
                if (_onException != null)
                    _onException((Exception)message["exception"]);   
            } else 
                if (message.ContainsKey("ext")) {
                    if (_onReceivedMetaData != null)
                        _onReceivedMetaData( (IDictionary<string, object>) message["ext"]);
                }
            return true;
        }

        public bool send(IClientSession session, IMutableMessage message) {
            //logger.Debug("send: " + message.JSON);
            return true;
        }

        public bool sendMeta(IClientSession session, IMutableMessage message) {
            //logger.Debug("sendMeta: " + message.JSON);
            return true;
        }
    }
}