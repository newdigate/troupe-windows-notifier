#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using Cometd.Bayeux;
using Cometd.Bayeux.Client;
using Cometd.Client;
using Cometd.Client.Transport;
using log4net;
using Troupe.Common.Interfaces;
using Troupe.WindowsApp.TroupeClient.Messages;

#endregion

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeSession : ITroupeSession {
        private static readonly ILog logger = LogManager.GetLogger(typeof (TroupeSession));
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        private readonly string _fayeUrl;
        private readonly IFactory<ITroupeAuthenticator> _authenticatorFactory;
        private readonly ManualResetEvent _signalComplete;
        private readonly AutoResetEvent _signalAuthComplete = new AutoResetEvent(false);
 
        private ITroupeAuthenticationToken _authToken;
        private ITroupeAuthenticator _authenticator;
        protected TroupeBayeuxClient client;
        private IClientSessionChannel userChannel;
        private IClientSessionChannel userChannelTroupes;
        private IClientSessionChannel userChannelInvites;
        private Thread sessionThread;

        private readonly Dictionary<string, ITroupeTopic> _topics = new Dictionary<string, ITroupeTopic>();
        private readonly Dictionary<string, ITroupeInvite> _invites = new Dictionary<string, ITroupeInvite>(); 
        
        private readonly List<ITroupeSessionListener> _listeners = new List<ITroupeSessionListener>();
        private readonly TroupeTopicExtractor _troupeTopicExtractor = new TroupeTopicExtractor();
        private readonly TroupeInviteExtractor _troupeInviteExtractor = new TroupeInviteExtractor();

        private SessionState _state;

        private TroupeUserSessionMessageListener _troupeUserSessionMessageListener;
        private TroupeUserTroupesSessionMessageListener _troupeUserTroupesSessionMessageListener;
        private TroupeUserInvitesMessageListener _troupeUserInvitesMessageListener;
        private readonly ISessionClientDataStore _sessionClientDataStore;

        #region public properties

        #endregion

        public TroupeSession(ITroupeAuthenticationToken authenticationToken, IFactory<ITroupeAuthenticator> authenticatorFactory, string fayeUrl, ManualResetEvent signalComplete, ISessionClientDataStore sessionClientDataStore) {
            _authenticatorFactory = authenticatorFactory;
            _sessionClientDataStore = sessionClientDataStore;
            _signalComplete = signalComplete;
            _fayeUrl = fayeUrl;
            _authToken = authenticationToken;
           
            _troupeUserSessionMessageListener = new TroupeUserSessionMessageListener(OnTroupeUserSessionMessage, OnTroupeUnreadMessages);
            _troupeUserTroupesSessionMessageListener = new TroupeUserTroupesSessionMessageListener(OnTroupeUserTroupesSessionMessage);
            _troupeUserInvitesMessageListener = new TroupeUserInvitesMessageListener(OnTroupeUserInviteReceived, OnTroupeUserInviteRemoved);
            logger.Info("troupe session created");
        }

        private void OnTroupeUserInviteReceived(ITroupeInvite invite) {
            AddInvite(invite);
        }
        
        private void OnTroupeUserInviteRemoved(ITroupeInvite invite)
        {
            RemoveInvite(invite.Id);
        }

        private void RemoveInvite(string id)
        {
            if (_invites.ContainsKey(id)) {
                ITroupeInvite invite = _invites[id];
                _invites.Remove(id);
                InvokeInviteRemoved(invite);
            }
        }

        private void OnTroupeUnreadMessages(string troupeId, int i) {
            InvokeUnreadMessages(troupeId, i);
        }

        private void OnTroupeUserSessionMessage(string topic, string title, string message) {
            TroupeNotificationMessage troupeNotificationMessage = new TroupeNotificationMessage(title, message, topic);
            InvokeNewMessage(topic, troupeNotificationMessage); 
        }

        private void OnTroupeUserTroupesSessionMessage(object o) {
        }

        #region ITroupeSession Members

        public void AddSessionListener(ITroupeSessionListener listener) {
            if ((listener != null) && (!_listeners.Contains(listener))) {
                _listeners.Add(listener);
            }
        }

        public void BeginSession() {
            if (_state != SessionState.Disconnected) {
                EndSession(false);
            }

            Join();

            logger.Info("creating worker threads");
      
            sessionThread = new Thread(RunThreadSession);
            sessionThread.Start();
        }

        private void RunThreadSession(object state) {
            logger.Info("RunThreadSession");
            _signalComplete.Reset();
            _signalAuthComplete.Reset();
            SessionAuthenticationLoop();
            logger.Info("END RunThreadSession");
        }

        public void EndSession(bool logout) {
            _topics.Clear();

            if (logout) {
                _sessionClientDataStore.DeleteSessionClientData();
                _authToken = null;
                logger.Info("Deleted session data");
            }

            logger.Info("EndSession() called");
            _signalComplete.Set();

            Join();

            InvokeLogoutCompleted();
            logger.Info("EndSession() complete");
        }

        private void Join() {

            if ((sessionThread != null) && (sessionThread.IsAlive))
                sessionThread.Join();

            sessionThread = null;
        }

        public void BringToFront() {
            if (_authenticator != null) {
                ((TroupeAuthenticator)_authenticator).BringToTop();
            }
        }
        #endregion

        #region private methods

        private void SessionAuthenticationLoop() {
            logger.Info("SessionAuthenticationLoop() called");
            if (!_signalComplete.WaitOne(TimeSpan.FromSeconds(1))) 
            try{
                    
                    if (_authToken == null)
                    try {
                        using (_authenticator = _authenticatorFactory.Create()) {
                            _state = SessionState.Authenticating;
                            _authToken = _authenticator.Authenticate("");
                        }
                        _authenticator = null;
                    } catch (Exception exc) {
                        _authToken = null;
                        _state = SessionState.Disconnected;
                        logger.Info(exc);
                        InvokeLoginCompleted(false);
                        return;
                    }

                    if (_authToken != null) {
                        logger.Info("authtoken receieved");
                        _signalAuthComplete.Set();
                        _state = SessionState.Connected;
                        InvokeHandshaking();

                        logger.Info("Received signal _signalAuthComplete... create bayeux client");
                        client = new TroupeBayeuxClient(_fayeUrl, _authToken.SessionToken, new List<ClientTransport>() { new LongPollingTransport(null) });

                        AutoResetEvent signalMetadataReceived = new AutoResetEvent(false);
                        AutoResetEvent signalExceptionOccured = new AutoResetEvent(false);
                        ManualResetEvent signalTopicsReceived = new ManualResetEvent(false);
                        AutoResetEvent signalInvitesReceived = new AutoResetEvent(false);

                        string userID = null;

                        IExtension _troupeExtention = new TroupeExtension(
                            onReceivedMetaData: dict => {
                                                            logger.InfoFormat("onReceivedMetaData: {0}", _serializer.Serialize(dict));
                                                            if (dict.ContainsKey("userId")) {
                                                                userID = (string)dict["userId"];
                                                                signalMetadataReceived.Set();
                                                                logger.InfoFormat("Received userID: {0}", userID);
                                                            }
                                                            else if (dict.ContainsKey("snapshot")) {
                                                                if (!signalTopicsReceived.WaitOne(0)) {
                                                                    logger.Info("Invite snapshot received");
                                                                    ReceiveTopicSnapshot(dict, signalTopicsReceived);
                                                                } else
                                                                {
                                                                    logger.Info("Invite snapshot received");
                                                                    ClearInvites();
                                                                    ReceiveInviteSnapshot(dict, signalInvitesReceived);
                                                                }
                                                            }
                                                        },
                            onException:        exc =>  {
                                                            if (exc.GetType() == typeof(WebException)) 
                                                                return;

                                                            signalExceptionOccured.Set();
                                                            logger.InfoFormat("onException: {0}", exc);
                                                        }, 
                            onSessionTopicDataReceived: o => {

                                    if (o.GetType() == typeof(Dictionary<string,object>)) {
                                        Dictionary<string, object> d1 = (Dictionary<string, object>)o;
                                        if (d1.ContainsKey("operation")) {
                                            string operation = (string) d1["operation"];
                                            switch (operation) {
                                                case "update":
                                                case "create": CreateOrUpdateTopic(d1); break;
                                                case "remove": RemoveTopic(d1); break;
                                                case "patch":  PatchTopic(d1); break;

                                                default :
                                                    logger.InfoFormat("Ignored operation {0}", operation);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            );

                        client.addExtension(_troupeExtention);
                        logger.Info("handshaking...");
                        client.handshake();
                        logger.Info("wait for connected...20000 ms");
                        BayeuxClient.State clientState = client.waitFor(20000, new List<BayeuxClient.State>() {BayeuxClient.State.CONNECTED});
                        if (clientState != BayeuxClient.State.CONNECTED) {
                            logger.InfoFormat("client state {0} after 20000ms; disconnecting", clientState);
                            client.abort();
                            client.disconnect();
                            client.waitFor(1000, new List<BayeuxClient.State>() { BayeuxClient.State.DISCONNECTED });
                            client = null;
                            _authToken = null;
                            InvokeLogoutCompleted();
                            _state = SessionState.Disconnected;
                            logger.InfoFormat("STATE: {0}", _state);
                            return;
                        }

                        logger.Info("wait for metadata, exception, or timeout... 20000 ms");
                        int selectedSignal = WaitHandle.WaitAny(new WaitHandle[] { signalMetadataReceived, signalExceptionOccured, _signalComplete }, 20000);
                        switch (selectedSignal)
                        {
                            case 0:

                                // Subscription to channels
                                string channelId = string.Format("/user/{0}", userID);
                                logger.InfoFormat("subscribing {0}", channelId);
                                userChannel = client.getChannel(channelId);
                                userChannel.subscribe(_troupeUserSessionMessageListener);

                                string userTroupeTopic = string.Format("/user/{0}/troupes", userID);
                                logger.InfoFormat("subscribing {0}", userTroupeTopic);
                                userChannelTroupes = client.getChannel(userTroupeTopic);
                                userChannelTroupes.subscribe(_troupeUserTroupesSessionMessageListener);

                                logger.Info("waiting  signalTopicsReceived");
                                int secondSignal = WaitHandle.WaitAny(new WaitHandle[] {signalTopicsReceived, _signalComplete});
                                if (secondSignal == 0) {
                                    logger.InfoFormat("got topics");
                                    
                                    InvokeLoginCompleted(true);

                                    string userTroupeInvites = string.Format("/user/{0}/invites", userID);
                                    logger.InfoFormat("subscribing {0}", userTroupeInvites);
                                    userChannelInvites = client.getChannel(userTroupeInvites);
                                    userChannelInvites.subscribe(_troupeUserInvitesMessageListener);
                                    int thirdSignal = WaitHandle.WaitAny(new WaitHandle[] { signalInvitesReceived, _signalComplete });
                                    if (thirdSignal == 0) {
                                        logger.InfoFormat("got invites");

                                        logger.Info("waiting  _signalComplete");
                                        _signalComplete.WaitOne();
                                    }
                                }
                               
                                break;

                            case 1:
                                //_signalComplete.Set();
                                logger.Info("caught exception...");
                                break;

                            case 2:
                                logger.Info("received  _signalComplete...");
                                break;
                        }
                        
                        logger.Info("Disconnect bayeux");
                        client.disconnect();
                        client.waitFor(1000, new List<BayeuxClient.State>() { BayeuxClient.State.DISCONNECTED });
                        client = null;

                        InvokeLogoutCompleted();
                        _state = SessionState.Disconnected;
                        logger.InfoFormat("STATE: {0}", _state);
                    } else {
                        _state = SessionState.Disconnected;
                        logger.InfoFormat("STATE: {0}", _state);
                    }
            } catch (Exception exc) {
                logger.Info(exc);
            }
            logger.Info("SessionAuthenticationLoop() complete");
        }

        private void ReceiveTopicSnapshot(IDictionary<string, object> dict, ManualResetEvent signalTopicsReceived) {
            ClearTopics();
            foreach (ITroupeTopic topic in _troupeTopicExtractor.ExtractTopics(dict)) {
                AddTopic(topic);
            }
            signalTopicsReceived.Set();
            logger.Info("topics added, set signalTopicsReceived...");
        }

        private void ReceiveInviteSnapshot(IDictionary<string, object> dict, AutoResetEvent signalInvitesReceived)
        {
           
            foreach (ITroupeInvite invite in _troupeInviteExtractor.ExtractInvites(dict)) {
                AddInvite(invite);
            }

            if (signalInvitesReceived != null) {
                signalInvitesReceived.Set();
                logger.Info("invites added, set signalInvitesReceived...");
            }
        }
        

        private void PatchTopic(Dictionary<string, object> d1) {
            Dictionary<string, object> dict = (Dictionary<string, object>)d1["model"];
            string patchId = (string)dict["id"];
            logger.InfoFormat("patch: {0}: {1}", patchId, _serializer.Serialize(dict));
            if (_topics.ContainsKey(patchId)) {
                ITroupeTopic topicToUpdate = _topics[patchId];
                UpdateTroupeTopicProperties(topicToUpdate, dict);
                InvokeTopicUpdate(topicToUpdate);
            }
            else {
                logger.WarnFormat("Could not patch topic as it was not present in collection {0}", patchId);
            }
        }

        private void RemoveTopic(Dictionary<string, object> d1) {
            Dictionary<string, object> dict = (Dictionary<string, object>)d1["model"];
            string topicId = (string)dict["id"];
            logger.InfoFormat("remove: {0}: {1}", topicId, _serializer.Serialize(dict));
            if (_topics.ContainsKey(topicId)) {
                ITroupeTopic topicToRemove = _topics[topicId];
                _topics.Remove(topicId);
                InvokeTopicRemoved(topicToRemove);
            } else {
                logger.WarnFormat("Could not remove topic as it was not present in collection {0}", topicId);
            }
        }

        private void CreateOrUpdateTopic(Dictionary<string, object> d1) {
            Dictionary<string, object> dictModel = (Dictionary<string, object>)d1["model"];
            string updateId = (string)dictModel["id"];
            logger.InfoFormat("create/update: {0}: {1}", updateId, _serializer.Serialize(dictModel));
            if (_topics.ContainsKey(updateId)) {
                logger.WarnFormat("Topic already exists {0}", updateId);
                ITroupeTopic topic = _topics[updateId];
                UpdateTroupeTopicProperties(topic, dictModel);
                InvokeTopicUpdate(topic);
            }
            else {
                ITroupeTopic newTopic = _troupeTopicExtractor.CreateTroupeTopic(dictModel);
                AddTopic(newTopic);
            }
        }

        private void UpdateTroupeTopicProperties(ITroupeTopic topicToUpdate, Dictionary<string, object> dictModel3) {
            foreach (string key in dictModel3.Keys) {
                if (dictModel3[key] is string)
                    _troupeTopicExtractor.SetProperty(key, (string)dictModel3[key], topicToUpdate);
                else if (dictModel3[key] is bool)
                    _troupeTopicExtractor.SetProperty(key, dictModel3[key].ToString(), topicToUpdate);
                else if (dictModel3[key] is int)
                    _troupeTopicExtractor.SetProperty(key, ((int)dictModel3[key]).ToString(), topicToUpdate);
            }
        }

        private void ClearTopics() {
            logger.Info("clearing all topics");
            _topics.Clear();
        }

        private void ClearInvites(){
            logger.Info("clearing all invites");
            _invites.Clear();
        }

        private void AddTopic(ITroupeTopic troupeTopic) {
            if (!_topics.ContainsKey(troupeTopic.Id)) {
                _topics.Add(troupeTopic.Id, troupeTopic);
                try {
                    InvokeTopicAdded(troupeTopic);
                } catch (Exception exc) {
                    logger.Error(exc);
                }
            } else {
                logger.WarnFormat("Session already contains topic {0}. Possible duplicate?", troupeTopic.Url);
            }
        }

        private void AddInvite(ITroupeInvite troupeTopic) {
            if (!_invites.ContainsKey(troupeTopic.Id)) {
                _invites.Add(troupeTopic.Id, troupeTopic);
                try {
                    InvokeInviteAdded(troupeTopic);
                } catch (Exception exc) {
                    logger.Error(exc);
                }
            } else {
                logger.WarnFormat("Session already contains topic {0}. Possible duplicate?", troupeTopic.AcceptUrl);
            }
        }

        private void InvokeInviteAdded(ITroupeInvite troupeTopic) {
            _listeners.ForEach(listener => {
                try {
                    listener.InviteAdded(troupeTopic);
                }
                catch (Exception exc) {
                    logger.Info(exc);
                }
            });
        }

        private void InvokeInviteRemoved(ITroupeInvite troupeTopic) {
            _listeners.ForEach(listener =>{
                try{
                    listener.InviteRemoved(troupeTopic);
                }
                catch (Exception exc) {
                    logger.Info(exc);
                }
            });
        }

        private void InvokeHandshaking() {
            _listeners.ForEach(listener => {
                                   try {
                                       listener.Handshaking();
                                   } catch (Exception exc) {
                                       logger.Info(exc);
                                   }
                               });
        }

        private void InvokeLoginCompleted(bool successful) {
            _listeners.ForEach(listener => {
                                   try {
                                       listener.LoginCompleted(successful);
                                   } catch (Exception exc) {
                                       logger.Info(exc);
                                   }
                               });
        }

        private void InvokeLogoutCompleted() {
            _listeners.ForEach(listener => {
                                   try {
                                       listener.LogoutCompleted();
                                   } catch (Exception exc) {
                                       logger.Info(exc);
                                   }
                               });
        }

        private void InvokeNewMessage(string key, ITroupeMessage m) {
            ITroupeTopic topic;
            if (_topics.TryGetValue(key, out topic)) {
                // found a corresponding topic
                _listeners.ForEach(listener => {
                                       try {
                                           listener.MessageReceived(topic, m);
                                       } catch (Exception exc) {
                                           logger.Info(exc);
                                       }
                                   });
            } else {
                // no corresponding topic, assume its an invite
                _listeners.ForEach(listener =>
                {
                    try
                    {
                        listener.InviteReceived(m);
                    }
                    catch (Exception exc)
                    {
                        logger.Info(exc);
                    }
                });
            }
        }

        private void InvokeTopicAdded(ITroupeTopic topic)
        {
            _listeners.ForEach(listener =>
            {
                try
                {
                    listener.TopicAdded(topic);
                }
                catch (Exception exc)
                {
                    logger.Info(exc);
                }
            });
        }

        private void InvokeTopicUpdate(ITroupeTopic topic)
        {
            _listeners.ForEach(listener =>
            {
                try
                {
                    listener.TopicUpdate(topic);
                }
                catch (Exception exc)
                {
                    logger.Info(exc);
                }
            });
        }

        private void InvokeTopicRemoved(ITroupeTopic topic)
        {
            _listeners.ForEach(listener =>
            {
                try
                {
                    listener.TopicRemoved(topic);
                }
                catch (Exception exc)
                {
                    logger.Info(exc);
                }
            });
        }

        private void InvokeUnreadMessages(string troupeId, int numUnreadMessages)
        {
            _listeners.ForEach(listener =>
            {
                try
                {
                    listener.UnreadMessages(troupeId, numUnreadMessages);
                }
                catch (Exception exc)
                {
                    logger.Info(exc);
                }
            });
        }

        #endregion
    }

    public enum SessionState {
        Disconnected = 0,
        Authenticating = 1,
        Connected = 2
    }
}