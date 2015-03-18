using System;
using System.Windows.Forms;
using log4net;
using Troupe.Common.Interfaces;
using Xilium.CefGlue.WindowsForms;
using System.Threading;

namespace Troupe.WindowsApp.Auth.Chromium
{
    public partial class LoginForm : Form {
        private readonly ILog logger = LogManager.GetLogger(typeof (LoginForm));
        private readonly string _startUrl;
        private readonly CefWebBrowser browser;

        private readonly ITroupeAuthTokenHandler _handler;

        private bool completed = false;

        public LoginForm(ITroupeAuthTokenHandler handler, string startUrl)
        {
            InitializeComponent();
            _startUrl = startUrl;
            browser = new CefWebBrowser();
            browser.StartUrl = _startUrl;
            browser.Dock = DockStyle.Fill;
            browser.AddressChanged +=new EventHandler(browser_AddressChanged);

            panel1.Controls.Add(browser);
            _handler = handler;

        }

        public void Go(string url)
        {
            var ctl = GetActiveBrowser();
            if (ctl != null)
            {
                ctl.Browser.GetMainFrame().LoadUrl(url);
            }
        }

        private void browser_AddressChanged(object sender, EventArgs e) {
            if (browser.Address == _startUrl) {
                //BeginInvoke(new Action(() => { WindowState = FormWindowState.Normal; }));
                return;
            }

            Uri uri = new Uri(browser.Address);
            if (uri.PathAndQuery.Contains("OAuthCallback?code=")) {
                string[] tokens = uri.PathAndQuery.Split(new string[] { "OAuthCallback?code=" }, StringSplitOptions.RemoveEmptyEntries);
                string token = tokens[1];
                completed = true;
                  this.BeginInvoke(new Action(Hide));
                  if (_handler != null) {
                      try {
                          _handler.InvokeNewAuthTokenEventHandler(token);
                      } catch (Exception exc) {
                          logger.Info(exc);
                      }
                  }
            } else if (uri.Fragment.Contains("signup")){
                Go(_startUrl);
                string url = uri.ToString();

                logger.InfoFormat("Launching browser to {0}.", url);
                ThreadPool.QueueUserWorkItem(o => {
                                                 logger.InfoFormat("About to launch {0}", url);
                                                 try {
                                                     System.Diagnostics.Process.Start(url);
                                                 } catch (Exception exc) {
                                                     logger.Error(string.Format("Exception occurred while launching {0}", url), exc);
                                                 }
                                             });
            }
        }

        private CefWebBrowser GetActiveBrowser() {

            foreach (var ctl in this.panel1.Controls) {
                if (ctl is CefWebBrowser) {
                    var browser = (CefWebBrowser) ctl;
                    return browser;
                }
            }

            return null;
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!completed) {
                completed = true;
                if (_handler != null)
                    _handler.InvokeAuthFailedEventHandler("Cancelled");

            }
        }

    }
}
