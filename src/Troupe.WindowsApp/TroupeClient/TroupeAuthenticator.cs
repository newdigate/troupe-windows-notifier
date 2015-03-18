#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using log4net;
using Troupe.Common.Interfaces;
using Troupe.WindowsApp.Auth.Chromium;

#endregion

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeAuthenticator : ITroupeAuthenticator {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TroupeAuthenticator));
        private const string _requestUriQuery =  "client_id={0}&client_secret={1}&grant_type=authorization_code&code={2}&redirect_uri=http%3A%2F%2Ftrou.pe%2FOAuthCallback";

        private readonly Form _parentForm; 
        private readonly ITroupeAuthTokenHandler _authTokenHandler;
        private readonly ITroupeClientConfig  _troupeClientConfig;
        private readonly ManualResetEvent _signalComplete;
        private IAsyncResult asyncResult = new AsyncResult();
        private LoginForm _loginForm;
        private readonly JavaScriptSerializer _jsonSerializer;
        private readonly ISessionClientDataStore _sessionClientDataStore;
        
        public TroupeAuthenticator(Form parentForm, ITroupeAuthTokenHandler authTokenHandler, ITroupeClientConfig troupeClientConfig, ManualResetEvent signalComplete, ISessionClientDataStore sessionClientDataStore) {
            _parentForm = parentForm;
            _sessionClientDataStore = sessionClientDataStore;
            _signalComplete = signalComplete;
            _troupeClientConfig = troupeClientConfig;
            _authTokenHandler = authTokenHandler;
            _authTokenHandler.OnNewAuthTokenReceived += _authTokenHandler_OnNewAuthTokenReceived;
            _authTokenHandler.OnAuthenticationFailed += _authTokenHandler_OnAuthenticationFailed;
            _jsonSerializer = new JavaScriptSerializer();
        }

        public void BringToTop() {
            if (_loginForm != null)
                _loginForm.Show();
                _loginForm.BringToFront();
        }

        void _authTokenHandler_OnAuthenticationFailed(object sender, AuthFailedEventArgs e)
        {
            ((AsyncResult)asyncResult).SetAsyncState(null);
            ((AutoResetEvent)asyncResult.AsyncWaitHandle).Set();
        }

        void _authTokenHandler_OnNewAuthTokenReceived(object sender, TokenEventArgs e) {
            string requestPostData = string.Format(_requestUriQuery, _troupeClientConfig.ClientKey, _troupeClientConfig.ClientSecret, e.Token);
            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(_troupeClientConfig.OAuthTokenUrl);
            client.Method = "POST";
            client.AuthenticationLevel = AuthenticationLevel.None;
            client.PreAuthenticate = false;
            client.ServicePoint.Expect100Continue = false;
            client.ContentType = "application/x-www-form-urlencoded";
            
            StreamWriter writer = new StreamWriter(client.GetRequestStream());
            writer.Write(requestPostData);
            writer.Flush();

            WebResponse response = null;
            try {
                response = client.GetResponse();
                string clientData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Dictionary<string, object> data = (Dictionary<string, object>)_jsonSerializer.DeserializeObject(clientData);
                if (data != null) {
                    if (data.ContainsKey("access_token")) {
                        ITroupeAuthenticationToken troupeAuthenticationToken = new TroupeAuthenticationToken() {
                            SessionToken = (string)data["access_token"],
                            ClientData = clientData
                        };
                        ((AsyncResult)asyncResult).SetAsyncState(troupeAuthenticationToken);

                        SaveAndEncryptSessionClientData(troupeAuthenticationToken);

                    }
                }
            } catch (Exception exc) {
                logger.Info(exc);
                ((AsyncResult)asyncResult).SetAsyncState(null);
            } finally {
               
                if (_loginForm != null) {
                    _parentForm.BeginInvoke(new Action(() => {
                                                        _loginForm.Close();
                                                    }));
                }
            }

            ((AutoResetEvent) asyncResult.AsyncWaitHandle).Set();
        }

        private void SaveAndEncryptSessionClientData(ITroupeAuthenticationToken token) {
            try {
                _sessionClientDataStore.SaveSessionClientData(token);                 
            } catch (Exception exc) {
                logger.Info(exc);    
            }
        }

        #region ITroupeAuthenticator Members

        public ITroupeAuthenticationToken Authenticate(string url) {
            asyncResult = BeginAuthenticate(url);
            ITroupeAuthenticationToken result = EndAuthenticate(asyncResult);
            return result;
        }

        public IAsyncResult BeginAuthenticate(string url) {
            _parentForm.BeginInvoke(new Action(ShowAuthForm));      
            return asyncResult;
        }

        public ITroupeAuthenticationToken EndAuthenticate(IAsyncResult result)
        {
            while(!result.AsyncWaitHandle.WaitOne(1000)) {
                if (_signalComplete.WaitOne(10))
                    return null;
            }

            TroupeAuthenticationToken token = (TroupeAuthenticationToken)result.AsyncState;
            return token;
        }

        private void ShowAuthForm() {
            string url = string.Format("{0}?client_id={1}&redirect_uri=http%3A%2F%2Ftrou.pe%2FOAuthCallback&response_type=code&scope=read", _troupeClientConfig.OAuthLoginUrl, _troupeClientConfig.ClientKey);
            _loginForm = new LoginForm(_authTokenHandler, url);
            _loginForm.Show();
        }

        public void Logout(ITroupeAuthenticationToken token) {
            if (token != null) {
                token.SessionToken = null;
            }
        }

        #endregion

        public void Dispose() {
            if (!_loginForm.IsDisposed && !_loginForm.Disposing) {
                _parentForm.BeginInvoke( new Action( () => _loginForm.Dispose()));
            }
        }
    }
}