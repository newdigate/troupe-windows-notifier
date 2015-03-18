using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using Microsoft.Win32;
using Troupe.Common.Config;
using Troupe.Common.Helpers;
using Troupe.Common.Interfaces;
using Troupe.WindowsApp.Helpers;
using Troupe.WindowsApp.TroupeClient;
using Troupe.WindowsApp.TroupeClient.Messages;
using Troupe.WindowsApp.WinForms;
using Zhwang.SuperNotifyIcon;
using AppLimit.NetSparkle;
using System.Collections;
using Timer = System.Windows.Forms.Timer;

namespace Troupe.WindowsApp
{
    public class TroupeApplicationContext : ApplicationContext, ITroupeSessionListener {

        private static readonly ILog logger = LogManager.GetLogger(typeof(TroupeApplicationContext));
        private static readonly XmlSerializer troupeConfigSerializer = new XmlSerializer(typeof(TroupeConfigSection));

        private const string _resourceNameIcoDisconnected   = "Resources\\icon-logo-disconnected.ico";
        private const string _resourceNameIcoAuthenticating = "Resources\\icon-logo-disconnected.ico";
        private const string _resourceNameIcoHanshaking     = "Resources\\icon-logo-disconnected.ico";
        private const string _resourceNameIcoConnected      = "Resources\\icon-logo-connected.ico";
        private const string _resourceNameIcoNewMessages    = "Resources\\icon-logo-connected-unread.ico";
        private const string _resourceNameIcoError          = "Resources\\icon-logo-connected-unread.ico";
        private const string _resourceNamePngFavorite       = "Resources\\FavoriteStar_FrontFacing_16x16_72.png";

        private static readonly TimeSpan _timespanSchduleMenuUpdateMilliseconds = TimeSpan.FromSeconds(1);

        private readonly IFactory<ITroupeAuthenticator> _authenticatorFactory;
        private readonly ITroupeClientConfig _clientConfig;
        private readonly ITroupeSession _session;

        private readonly NotifyIcon _notifyIcon;
        private readonly SuperNotifyIcon _extensions;
        private readonly Sparkle _sparkle;

        private ApplicationState _applicationState;

        private ToolStripMenuItem _menuItemExit, _menuItemLogin, _menuItemLogout, _menuItemCheckForUpdates;
        private ToolStripSeparator _separator1,_separator2,_separator3,_separator4,_separator5,_separator6,_separator7,_separator8, _separator9;

        private readonly Icon _icoDisconnected, _icoAuthenticating, _icoHandshaking, _icoConnected, _icoNewMessage, _icoError;

        private readonly TroupeSplashScreen _splashScreen;
        private readonly UpdatesForm _updatesForm;
        private readonly ContextMenuStrip _mainMenu;
        private readonly List<BalloonForm> _balloonForms = new List<BalloonForm>();
        private readonly ISessionClientDataStore _sessionClientDataStore;

        private bool _exiting;
        private bool _usingChrome = true;
        private CancellationTokenSource _databindMenusCancellationTokenSource;
        private CancellationTokenSource _updateUnreadMessagesStateCancellationTokenSource;

        // INVITES, UNREAD, FAVOURITES, RECENT, TROUPES, PRIVATE CHATS
        private ToolStripMenuItem _menuItemTitleINVITES, _menuItemTitleUNREAD, _menuItemTitleFAVOURITES, _menuItemTitleRECENT, _menuItemTitleTROUPES, _menuItemTitlePRIVATE_CHATS;

        private Dictionary<string, ITroupeTopic> _topics = new Dictionary<string, ITroupeTopic>();
        private Dictionary<string, ITroupeInvite> _invites = new Dictionary<string, ITroupeInvite>();

        private Dictionary<ITroupeTopic, ToolStripMenuItem> _topicMenuItems = new Dictionary<ITroupeTopic, ToolStripMenuItem>();
        private Dictionary<ITroupeInvite, ToolStripMenuItem> _inviteMenuItems = new Dictionary<ITroupeInvite, ToolStripMenuItem>();

        private readonly Font _menuItemFontBold;
        private Image _favoriteImage;

        public ApplicationState ApplicationState {
            get { return _applicationState; }
            set {
                if (value != _applicationState) {
                    _applicationState = value;

                    UpdateAppNotificationIcon();
                }
            }
        }

        public TroupeApplicationContext() {

            logger.Info("Troupe for Windows starting...");

            


            ManualResetEvent _signalComplete = new ManualResetEvent(false);

            _clientConfig = GetTroupeConfig();

            _sparkle = CreateSparkle(_clientConfig.Sparkle);
            _sparkle.updateDetected += new UpdateDetected(_sparkle_updateDetected);

            ITroupeAuthTokenHandler authTokenHandler = new TroupeAuthTokenHandler();

            _splashScreen = new TroupeSplashScreen();
            _splashScreen.ShowInTaskbar = false;
            _splashScreen.Show();
            _splashScreen.Hide();

            if (string.IsNullOrEmpty(_clientConfig.EnvironmentName)) {
                logger.Warn("EnvironmentName has not been set in config.");
            }

             _sessionClientDataStore = new WinRegistrySessionClientDataStore(_clientConfig.EnvironmentName);

             _authenticatorFactory = new TroupeAuthenticatorFactory(_splashScreen, authTokenHandler, _clientConfig, _signalComplete, _sessionClientDataStore);

            ITroupeAuthenticationToken authenticationToken = _sessionClientDataStore.GetClientSessionData();

            _session = new TroupeSession(authenticationToken, _authenticatorFactory, _clientConfig.Faye, _signalComplete, _sessionClientDataStore);

            _session.AddSessionListener(this);

            _icoDisconnected = new Icon(GetAbsoluteFilename(_resourceNameIcoDisconnected));
             _icoAuthenticating = new Icon(GetAbsoluteFilename(_resourceNameIcoAuthenticating));
             _icoHandshaking = new Icon(GetAbsoluteFilename(_resourceNameIcoHanshaking));
             _icoConnected = new Icon(GetAbsoluteFilename(_resourceNameIcoConnected));
             _icoNewMessage = new Icon(GetAbsoluteFilename(_resourceNameIcoNewMessages));
             _icoError= new Icon(GetAbsoluteFilename(_resourceNameIcoError));
             _favoriteImage = new Bitmap(GetAbsoluteFilename(_resourceNamePngFavorite));

             _mainMenu = CreateMenus();
             DataBindMenu();

            _notifyIcon = CreateNotifyIcon(out _extensions);
            _notifyIcon.ContextMenuStrip = _mainMenu;
            _session.BeginSession();

            _updatesForm = new UpdatesForm(_sparkle);
            TimeSpan checkFrequency = new TimeSpan(0, 0, 86400);
            _sparkle.StartLoop(true, checkFrequency);

            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
            _menuItemFontBold = new Font(_mainMenu.Font.FontFamily, _menuItemLogout.Font.Size, FontStyle.Bold);

            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            UserPresenceFacade userPresenceFacade = new UserPresenceFacade();
            userPresenceFacade.UserActive = () => { if (_session != null) _session.BeginSession(); };
            userPresenceFacade.UserInactive = () => { if (_session != null) _session.EndSession(false); };
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            logger.InfoFormat("PowerMode changed to {0}", e.Mode);
            switch (e.Mode) {

                case PowerModes.Suspend:
                    if (_session != null)
                        _session.EndSession(false);
                    break;

                case PowerModes.Resume:
                    if (_session != null)
                        _session.BeginSession();
                    break;
                default:
                    break;
            }
        }

        void Application_ApplicationExit(object sender, EventArgs e) {
            _notifyIcon.Icon = null;
            _notifyIcon.Dispose();
        }

        private Sparkle CreateSparkle(string sparkleUrl)
        {
            Sparkle result = new Sparkle(sparkleUrl)
                       {
                           ShowDiagnosticWindow = false,
                           TrustEverySSLConnection = true,
                           EnableServiceMode = false,
                           EnableSilentMode = false,
                           SilentInstall = true
                       };
            return result;
        }

        void _sparkle_updateDetected(object sender, UpdateDetectedEventArgs e) {
            e.NextAction = nNextUpdateAction.showStandardUserInterface;
        }

        static public TroupeConfigSection GetTroupeConfig()
        {
            string s = ConfigurationManager.AppSettings[_appSettingKey_troupeConfig];
            if (!string.IsNullOrEmpty(s))
            {
                try {
                    byte[] base64 = Convert.FromBase64String(s);
                    string decrypted = Encryption.DecryptStringFromBytes(base64, Encryption.Common_Key, Encryption.Common_iv);

                    XmlTextReader xmlReader = new XmlTextReader(new StringReader(decrypted));
                    TroupeConfigSection result = (TroupeConfigSection)troupeConfigSerializer.Deserialize(xmlReader);
                    return result;
                } catch (Exception exc) {
                    logger.Error("Could not load config.", exc);
                }
            }
            else logger.ErrorFormat("app setting key={0} was not found.", _appSettingKey_troupeConfig);

            return null;
        }

        #region private ui methods

        private NotifyIcon CreateNotifyIcon(out SuperNotifyIcon extensions)
        {
            NotifyIcon notifyIcon = new NotifyIcon { Visible = true };
            notifyIcon.Icon = _icoDisconnected;

            notifyIcon.MouseUp += notifyIcon1_MouseUp;

            extensions = new SuperNotifyIcon { NotifyIcon = notifyIcon };
            return notifyIcon;
        }

        private string GetAbsoluteFilename(string filename) {
            return Path.Combine( Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), filename);
        }

        private ContextMenuStrip CreateMenus()
        {
            _separator1 = new ToolStripSeparator();
            _separator2 = new ToolStripSeparator();
            _separator3 = new ToolStripSeparator();
            _separator4 = new ToolStripSeparator();
            _separator5 = new ToolStripSeparator();
            _separator6 = new ToolStripSeparator();
            _separator7 = new ToolStripSeparator();
            _separator8 = new ToolStripSeparator();
            _separator9 = new ToolStripSeparator();

            _menuItemExit = new ToolStripMenuItem("Exit");
            _menuItemExit.Click += MenuItemExitClick;

            _menuItemLogin = new ToolStripMenuItem("Login");
            _menuItemLogin.Click += menuItemLogin_Click;

            _menuItemLogout = new ToolStripMenuItem("Logout");
            _menuItemLogout.Click += menuItemLogout_Click;
            _menuItemLogout.Visible = false;

            _menuItemCheckForUpdates = new ToolStripMenuItem("Check for updates");
            _menuItemCheckForUpdates.Click += new EventHandler(_menuItemCheckForUpdates_Click);

            // INVITES, UNREAD, FAVOURITES, RECENT, TROUPES, PRIVATE CHATS
            _menuItemTitleINVITES = new ToolStripMenuItem("INVITES") { Enabled = false };
            _menuItemTitleUNREAD = new ToolStripMenuItem("UNREAD") { Enabled = false };
            _menuItemTitleFAVOURITES = new ToolStripMenuItem("FAVOURITES") {Enabled = false, Image = _favoriteImage};
            _menuItemTitleRECENT = new ToolStripMenuItem("RECENT") { Enabled = false };
            _menuItemTitleTROUPES = new ToolStripMenuItem("TROUPES");
            _menuItemTitlePRIVATE_CHATS = new ToolStripMenuItem("PRIVATE CHATS");

            ContextMenuStrip mainMenu = new ContextMenuStrip();
            return mainMenu;
        }

        private int debugNumber = 1;
        private static readonly string _appSettingKey_troupeConfig = "Troupe.config";

        private void ScheduleMenuDataBind() {
            int debugInstance = debugNumber++;
            logger.InfoFormat("Scheduling #{1} menu update in {0} ms", _timespanSchduleMenuUpdateMilliseconds.TotalMilliseconds, debugInstance);
            if (_databindMenusCancellationTokenSource != null)
                _databindMenusCancellationTokenSource.Cancel();

            _databindMenusCancellationTokenSource = new CancellationTokenSource();

            Observable.Interval(_timespanSchduleMenuUpdateMilliseconds, new ControlScheduler(_splashScreen))
                .Subscribe(
                            (s) => {
                               try {
                                   _databindMenusCancellationTokenSource.Cancel();
                                   DataBindMenu();
                               } catch (Exception exc) {
                                   logger.Warn(string.Format("exception onNext: {0}", debugInstance), exc);
                               }
                           },
                           (exc) => {
                               logger.Info(string.Format("exception {0}", debugInstance), exc);
                           },
                           () => {
                               logger.Info(string.Format("menu update cancelled {0}", debugInstance));
                           }, _databindMenusCancellationTokenSource.Token);
        }

        private void DataBindMenu() {
            logger.Info("performing menu update");
            if (_splashScreen.InvokeRequired)
                _splashScreen.BeginInvoke(new Action(DataBindMenuInvoked));
            else
                DataBindMenuInvoked();
        }

        private void DataBindMenuInvoked() {

            List<ToolStripItem> allItems = new List<ToolStripItem>();

            #region INVITES
            IList<ToolStripMenuItem> inviteMenuItems = GetInvitesMenuItems();
            if (inviteMenuItems.Count > 0) {
                allItems.Add(_menuItemTitleINVITES);
                allItems.AddRange(inviteMenuItems);
                allItems.Add(_separator1);
            }

            #endregion

            #region UNREAD

            List<ITroupeTopic> unreadTopics = new List<ITroupeTopic>(
                _topics.Values.Where(topic => {
                                         int unread;
                                         return (!string.IsNullOrEmpty(topic.UnreadItems)
                                                 && int.TryParse(topic.UnreadItems, out unread)
                                                 && unread > 0);
                }));

            if (unreadTopics.Count > 0) {
                allItems.Add(_menuItemTitleUNREAD);
                SortedList<string, ITroupeTopic> sortedUnreadTopics = new SortedList<string, ITroupeTopic>();
                unreadTopics.ForEach(unreadTopic => sortedUnreadTopics.Add(unreadTopic.Name, unreadTopic));
                foreach (ITroupeTopic unreadTopic in sortedUnreadTopics.Values) {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(unreadTopic.Name.Replace("&","&&"));
                    menuItem.Click += topicMenuItem_Click;
                    UpdateTroupeTopicMenuItem(unreadTopic, menuItem);
                    menuItem.Font = _menuItemFontBold;
                    allItems.Add(menuItem);
                }
                allItems.Add(_separator2);
            }

            #endregion



            #region FAVOURITES
            List<ITroupeTopic> favouriteTopics = new List<ITroupeTopic>(
                _topics.Values.Where(topic =>
                {
                    bool fav;
                    return (!string.IsNullOrEmpty(topic.UnreadItems)
                            && bool.TryParse(topic.Favourite, out fav)
                            && fav);
                }));

            if (favouriteTopics.Count > 0) {
                allItems.Add(_menuItemTitleFAVOURITES);
                SortedList<string, ITroupeTopic> sortedFavorites = new SortedList<string, ITroupeTopic>();
                favouriteTopics.ForEach(favTopic => sortedFavorites.Add(favTopic.Name, favTopic));
                foreach (ITroupeTopic favTopic in sortedFavorites.Values) {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(favTopic.Name.Replace("&","&&"));
                    menuItem.Click += topicMenuItem_Click;
                    UpdateTroupeTopicMenuItem(favTopic, menuItem);
                    menuItem.Image = _favoriteImage;
                    allItems.Add(menuItem);
                }
                allItems.Add(_separator3);
            }
            #endregion

            #region RECENT

            List<ITroupeTopic> recentTopics = new List<ITroupeTopic>(_topics.Values);
            recentTopics.Sort(new Comparison<ITroupeTopic>( (t,t2) => {
                                                                DateTime dt = DateTime.MinValue;
                                                                if (t.LastAccessTime != null)
                                                                    dt = DateTime.ParseExact(t.LastAccessTime, "yyyy-MM-ddTHH:mm:ss.fffZ", null);

                                                                DateTime dt2 = DateTime.MinValue;
                                                                if (t2.LastAccessTime != null)
                                                                    dt2 = DateTime.ParseExact(t2.LastAccessTime, "yyyy-MM-ddTHH:mm:ss.fffZ", null);

                                                                return dt2.CompareTo(dt);
            }));
            if (recentTopics.Count > 0) {
                allItems.Add(_menuItemTitleRECENT);
                IEnumerable<ITroupeTopic> top5RecentTroupes = recentTopics.Take(5);
                foreach (ITroupeTopic top5RecentTroupe in top5RecentTroupes) {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(top5RecentTroupe.Name.Replace("&","&&"));
                    menuItem.Click += topicMenuItem_Click;
                    UpdateTroupeTopicMenuItem(top5RecentTroupe, menuItem);
                    allItems.Add(menuItem);
                }
                allItems.Add(_separator4);
            }
            #endregion

            bool troupesOrPrivateChats = false;

            #region TROUPES

            IList<ToolStripMenuItem> troupeMenuItems = GetTroupeMenuItems();
            if (troupeMenuItems.Count > 0) {
                _menuItemTitleTROUPES.DropDown = new ToolStripDropDownMenu();
                allItems.Add(_menuItemTitleTROUPES);
                foreach (ToolStripMenuItem menuItem in troupeMenuItems) {
                    if (!_menuItemTitleTROUPES.DropDownItems.Contains(menuItem))
                        _menuItemTitleTROUPES.DropDownItems.Add(menuItem);
                }
                troupesOrPrivateChats = true;
            }
            #endregion

            #region PRIVATE CHATS
            IList<ToolStripMenuItem> privateChatsMenuItems = GetOneToOneMenuItems();
            if (privateChatsMenuItems.Count > 0) {
                allItems.Add(_menuItemTitlePRIVATE_CHATS);
                _menuItemTitlePRIVATE_CHATS.DropDown = new ToolStripDropDownMenu();
                foreach (ToolStripMenuItem menuItem in privateChatsMenuItems) {
                    if (!_menuItemTitlePRIVATE_CHATS.DropDownItems.Contains(menuItem))
                        _menuItemTitlePRIVATE_CHATS.DropDownItems.Add(menuItem);
                }
                troupesOrPrivateChats = true;
            }
            #endregion

            if (troupesOrPrivateChats)
                allItems.Add(_separator6);

            allItems.Add(_menuItemCheckForUpdates);
            allItems.Add(_menuItemLogin);
            allItems.Add(_menuItemLogout);

            allItems.Add(_separator7);

            allItems.Add(_menuItemExit);

            ToolStripItem[] toolStripItems = allItems.ToArray();
            _mainMenu.Items.Clear();
            _mainMenu.Items.AddRange(toolStripItems);
        }

        private IList<ToolStripMenuItem> GetTroupeMenuItems() {
            SortedList<string, ToolStripMenuItem> sortedTroupeMenuItems = new SortedList<string, ToolStripMenuItem>();

            foreach (string troupeId in _topics.Keys) {
                ITroupeTopic topic = _topics[troupeId];
                if ((string.IsNullOrEmpty(topic.OneToOne)) || ("False" == topic.OneToOne)) {
                    if (_topicMenuItems.ContainsKey(topic)) {
                        ToolStripMenuItem menuItem = _topicMenuItems[topic];
                        sortedTroupeMenuItems.Add(topic.Name, menuItem);
                    }
                    else logger.WarnFormat("Regular: did not find corresponding menu item for troupe id {0}", troupeId);
                }
            }
            return sortedTroupeMenuItems.Values;
        }

        private IList<ToolStripMenuItem> GetOneToOneMenuItems() {
            SortedList<string, ToolStripMenuItem> sortedOneToOneMenuItems = new SortedList<string, ToolStripMenuItem>();

            foreach (string troupeId in _topics.Keys) {
                ITroupeTopic topic = _topics[troupeId];
                if ("False" != topic.OneToOne) {
                    if (_topicMenuItems.ContainsKey(topic)) {
                        ToolStripMenuItem menuItem = _topicMenuItems[topic];
                        sortedOneToOneMenuItems.Add(topic.Name, menuItem);
                    } else logger.WarnFormat("OneToOne: did not find corresponding menu item for troupe id {0}", troupeId);
                }
            }
            return sortedOneToOneMenuItems.Values;
        }

        private IList<ToolStripMenuItem> GetInvitesMenuItems() {
            SortedList<string, ToolStripMenuItem> sortedInviteMenuItems = new SortedList<string, ToolStripMenuItem>();
            foreach (string troupeId in _invites.Keys) {
                ITroupeInvite topic = _invites[troupeId];
                    if (_inviteMenuItems.ContainsKey(topic)) {
                        ToolStripMenuItem menuItem = _inviteMenuItems[topic];
                        sortedInviteMenuItems.Add(topic.Name, menuItem);
                    } else logger.WarnFormat("Invite: did not find corresponding menu item for troupe id {0}", troupeId);
                }
            return sortedInviteMenuItems.Values;
        }

        private void _menuItemCheckForUpdates_Click(object sender, EventArgs e) {
            _updatesForm.Show();
        }

        private void UpdateAppNotificationIcon()
        {
            if (!_exiting)
            {

                switch (_applicationState)
                {
                    case ApplicationState.LoggedIn:
                        _splashScreen.BeginInvoke(new Action<Icon>(SetIcon), _icoConnected);
                        _splashScreen.BeginInvoke(new Action(() =>
                        {
                            _menuItemLogout.Visible = true;
                            _menuItemLogin.Visible = false;
                        }));

                        break;

                    case ApplicationState.Authenticating:
                        _splashScreen.BeginInvoke(new Action<Icon>(SetIcon), _icoAuthenticating);
                        _splashScreen.BeginInvoke(new Action(() =>
                        {
                            _menuItemLogout.Visible = true;
                            _menuItemLogin.Visible = false;
                        }));

                        break;
                    case ApplicationState.LoggedOut:
                        _splashScreen.BeginInvoke(new Action<Icon>(SetIcon), _icoDisconnected);
                        _splashScreen.BeginInvoke(new Action(() =>
                        {
                            _menuItemLogout.Visible = false;
                            _menuItemLogin.Visible = true;
                        }));

                        break;
                    case ApplicationState.NewNotification:
                        _splashScreen.BeginInvoke(new Action<Icon>(SetIcon), _icoNewMessage);
                        break;

                    case ApplicationState.Handshaking:
                        _splashScreen.BeginInvoke(new Action<Icon>(SetIcon), _icoHandshaking);
                        break;

                    case ApplicationState.Error:
                        _splashScreen.BeginInvoke(new Action<Icon>(SetIcon), _icoError);
                        break;
                }
            }
        }

        #endregion

        #region event handlers

        private void notifyIcon1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(_notifyIcon, null);
            }
        }

        private void MenuItemExitClick(object sender, EventArgs e) {
            _exiting = true;
            _sparkle.StopLoop();
            _sparkle.Reset();
            _session.EndSession(false);
            if (_updatesForm != null) _updatesForm.Kill();
            _splashScreen.Close();
            Application.Exit();
        }

        void menuItemLogout_Click(object sender, EventArgs e)
        {
            if (ApplicationState != ApplicationState.LoggedOut) {
                _session.EndSession(true);
            }
        }

        void menuItemLogin_Click(object sender, EventArgs e)
        {
            if (ApplicationState == ApplicationState.LoggedOut)
            {
                _session.BeginSession();
            }

        }

        #endregion

        public void Handshaking() {
            ApplicationState = ApplicationState.Handshaking;
        }

        public void MessageReceived(ITroupeTopic topic, ITroupeMessage message) {
            _splashScreen.BeginInvoke(new Action(() => {
                ClosePreviousBalloonWindows();


                if (message is TroupeNotificationMessage)
                {
                    TroupeNotificationMessage troupeNotificationMessage = message as TroupeNotificationMessage;
                    BalloonForm sessionBalloonForm = new BalloonForm(_extensions, new ControlScheduler(_splashScreen));
                    sessionBalloonForm.ShowNotification(troupeNotificationMessage.Title, troupeNotificationMessage.Message);
                    sessionBalloonForm.SetTimeout(5000);
                    sessionBalloonForm.Click += new EventHandler(sessionBalloonForm_Click);
                    sessionBalloonForm.Tag = topic;

                    lock (_balloonForms) {
                        _balloonForms.Add(sessionBalloonForm);
                    }

                    ToolStripMenuItem menuItem;
                    if (_topicMenuItems.TryGetValue(topic, out menuItem)) {
                        menuItem.Font = _menuItemFontBold;
                    }
                }
            }));
        }

        public void InviteReceived(ITroupeMessage message)
        {
            _splashScreen.BeginInvoke(new Action(() =>
            {
                ClosePreviousBalloonWindows();

                if (message is TroupeNotificationMessage)
                {
                    TroupeNotificationMessage troupeNotificationMessage = message as TroupeNotificationMessage;
                    BalloonForm inviteBalloonForm = new BalloonForm(_extensions, new ControlScheduler(_splashScreen));
                    inviteBalloonForm.ShowNotification(troupeNotificationMessage.Title, troupeNotificationMessage.Message);
                    inviteBalloonForm.SetTimeout(7000);
                    inviteBalloonForm.Click += new EventHandler(inviteBalloonForm_Click);
                    inviteBalloonForm.Tag = troupeNotificationMessage.Link;

                    lock (_balloonForms) {
                        _balloonForms.Add(inviteBalloonForm);
                    }

                }
            }));
        }

        public void InviteAdded(ITroupeInvite invite) {

            if (string.IsNullOrEmpty(invite.Id)) {
                logger.Warn("topic.Id is null");
                return;
            }

            if (!_invites.ContainsKey(invite.Id)) {
                _invites.Add(invite.Id, invite);
                logger.InfoFormat("Added topic id={0} url={1} name={2}", invite.Id, invite.AcceptUrl, invite.Name);

                if (!_inviteMenuItems.ContainsKey(invite)) {
                    _splashScreen.BeginInvoke(new Action(() => {
                        ToolStripMenuItem inviteMenuItem = new ToolStripMenuItem(invite.Name.Replace("&","&&"));

                        inviteMenuItem.Tag = invite;

                        _inviteMenuItems.Add(invite, inviteMenuItem);
                        inviteMenuItem.Click += topicMenuItem_Click;
                    }));
                }
            }
            else
                logger.InfoFormat("Already added invite {0}", invite.Id);

            ScheduleMenuDataBind();
        }

        public void InviteRemoved(ITroupeInvite invite) {
            if (_invites.ContainsKey(invite.Id)) {
                _invites.Remove(invite.Id);

                if (_inviteMenuItems.ContainsKey(invite)) {
                    _inviteMenuItems.Remove(invite);
                }
            }
            ScheduleMenuDataBind();
        }

        void inviteBalloonForm_Click(object sender, EventArgs e)
        {
            logger.Info("Invite window was clicked.");
            if (sender is BalloonForm)
            {
                BalloonForm item = sender as BalloonForm;
                string topic = (string)item.Tag;
                string url = string.Format("{0}{1}", _clientConfig.BaseUrl, topic);
                LaunchBrowser(url);
                item.Hide();
            }
        }

        private void ClosePreviousBalloonWindows() {
            lock (_balloonForms) {
                foreach (BalloonForm balloonForm in _balloonForms) {
                    balloonForm.ClearTimeout();
                    balloonForm.Close();
                }
                _balloonForms.Clear();
            }
        }

        void sessionBalloonForm_Click(object sender, EventArgs e)
        {
            logger.Info("Notification window was clicked.");
            if (sender is BalloonForm)
            {
                BalloonForm item = sender as BalloonForm;
                ITroupeTopic topic = (ITroupeTopic)item.Tag;
                string url = string.Format("{0}{1}",_clientConfig.BaseUrl, topic.Url);
                LaunchBrowser(url);
                item.Hide();
            }
        }

        private void LaunchBrowser(string url) {
            logger.InfoFormat("Launching browser to {0}.", url);
            ThreadPool.QueueUserWorkItem(o => {
                                             if (_usingChrome) {
                                                 logger.InfoFormat("About to launch chrome.exe {0}", url);
                                                 try {
                                                     System.Diagnostics.Process.Start("chrome.exe", url);
                                                     return;
                                                 } catch (Exception exc) {
                                                     logger.Error("Exception occurred while launching chrome.exe", exc);
                                                     _usingChrome = false;
                                                 }
                                             }

                                             logger.InfoFormat("About to launch {0}", url);
                                             try {
                                                 System.Diagnostics.Process.Start(url);
                                             } catch (Exception exc) {
                                                 logger.Error(string.Format("Exception occurred while launching {0}", url), exc);
                                             }
                                         });
        }

        public void TopicAdded(ITroupeTopic topic) {
            if (string.IsNullOrEmpty(topic.Url)) {
                logger.Warn("topic.Url is null");
                return;
            }

            if (_invites.ContainsKey(topic.Id))
                _invites.Remove(topic.Id);

            if (!_topics.ContainsKey(topic.Id)) {
                _topics.Add(topic.Id, topic);
                logger.InfoFormat("Added topic id={0} url={1} name={2}", topic.Id, topic.Url, topic.Name);

                if (!_topicMenuItems.ContainsKey(topic)) {
                    _splashScreen.BeginInvoke(new Action(() => {
                        ToolStripMenuItem topicMenuItem = new ToolStripMenuItem(topic.Name.Replace("&", "&&"));

                                                             UpdateTroupeTopicMenuItem(topic, topicMenuItem);

                                                             _topicMenuItems.Add(topic, topicMenuItem);
                                                             topicMenuItem.Click += topicMenuItem_Click;

                                                             ScheduleMenuDataBind();
                                                         }
                                                  ));
                }
            } else
                logger.InfoFormat("Already added topic {0}", topic.Id);

            ScheduleUpdateUnreadMessagesState();
        }

        private void UpdateTroupeTopicMenuItem(ITroupeTopic topic, ToolStripMenuItem topicMenuItem) {
            topicMenuItem.Tag = topic;
            topicMenuItem.Text = topic.Name;
        }

        public void TopicUpdate(ITroupeTopic topic) {
            if (_topics.ContainsKey(topic.Id)) {
                if (_topicMenuItems.ContainsKey(topic)) {
                    ToolStripMenuItem toolStripMenuItem = _topicMenuItems[topic];
                    UpdateTroupeTopicMenuItem(topic, toolStripMenuItem);
                    ScheduleMenuDataBind();
                }
            }
            ScheduleUpdateUnreadMessagesState();
        }

        public void TopicRemoved(ITroupeTopic topic) {
            if (_topics.ContainsKey(topic.Id)) {
                _topics.Remove(topic.Id);
                if (_topicMenuItems.ContainsKey(topic)) {
                    ToolStripMenuItem topicMenuItem = _topicMenuItems[topic];
                    _topicMenuItems.Remove(topic);
                    _splashScreen.BeginInvoke(new Action(() => {
                                                             _mainMenu.Items.Remove(topicMenuItem);
                                                         }
                                                  ));

                }


            } else {
                logger.WarnFormat("Unable to remove topic {0}", topic.Id);
            }
            ScheduleUpdateUnreadMessagesState();
        }

        void topicMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem) {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                string url = null;
                if (item.Tag is ITroupeTopic) {
                    ITroupeTopic topic = (ITroupeTopic)item.Tag;
                    url = string.Format("{0}{1}", _clientConfig.BaseUrl, topic.Url);
                }
                else if (item.Tag is ITroupeInvite) {
                    ITroupeInvite invite = (ITroupeInvite)item.Tag;
                    url = string.Format("{0}{1}", _clientConfig.BaseUrl, invite.AcceptUrl);
                }
                LaunchBrowser(url);
            }
        }

        public void UnreadMessages(string troupeId, int numUnreadMessages) {
            logger.InfoFormat("UnreadMessages {0}={1}", troupeId, numUnreadMessages);

            if (_topics.ContainsKey(troupeId)) {
                ITroupeTopic topic = _topics[troupeId];
                topic.UnreadItems = numUnreadMessages.ToString();
                ToolStripMenuItem topicMenuItem;
                if (_topicMenuItems.TryGetValue(topic, out topicMenuItem)) {
                    _splashScreen.BeginInvoke(new Action(() => {
                                                             UpdateTroupeTopicMenuItem(topic, topicMenuItem);
                                                         }
                                                  ));
                } else
                    logger.WarnFormat("Could not find menu item associated with topic... {0} {1}", troupeId, numUnreadMessages);
            } else
                logger.WarnFormat("Received unread message notification which we dont appear to be subscribed to... {0} {1}", troupeId, numUnreadMessages);

            ScheduleUpdateUnreadMessagesState();
            ScheduleMenuDataBind();
        }

        private void ScheduleUpdateUnreadMessagesState() {
            if (_updateUnreadMessagesStateCancellationTokenSource != null)
                _updateUnreadMessagesStateCancellationTokenSource.Cancel();

            _updateUnreadMessagesStateCancellationTokenSource = new CancellationTokenSource();
            Observable.Interval(TimeSpan.FromMilliseconds(500), new ControlScheduler(_splashScreen))
                .Subscribe((s) => {
                               try {
                                   _updateUnreadMessagesStateCancellationTokenSource.Cancel();
                                   UpdateUnreadMessagesState();
                               } catch (Exception exc) {
                                   logger.Warn("exception onNext:", exc);
                               }
                           }, _updateUnreadMessagesStateCancellationTokenSource.Token);
        }

        private void UpdateUnreadMessagesState() {
            IEnumerable<ITroupeTopic> unreadTopics = _topics.Values.Where(t => (t.UnreadItems != "0"));
            IEnumerable<ITroupeTopic> readTopics = _topics.Values.Where(t => (t.UnreadItems == "0"));
            int unread = unreadTopics.Count();
            int read = readTopics.Count();
            logger.InfoFormat("Topics: total={0} read={1} unread={2}", _topics.Count, read, unread);
            if (unreadTopics.Count() == 0) {
                logger.InfoFormat("No unread items - set application state back to LoggedIn");
                ApplicationState = ApplicationState.LoggedIn;
            } else {
                logger.InfoFormat("{0} unread items (applicationState=NewNotification)", unread);
                ApplicationState = ApplicationState.NewNotification;
                foreach (ITroupeTopic unreadTopic in unreadTopics) {
                    logger.InfoFormat("{0} - {1}", unreadTopic.Name, unreadTopic.UnreadItems?? "null");
                }
            }
        }

        public void LoginCompleted(bool successful)
        {
            if (successful) {

                ApplicationState = ApplicationState.LoggedIn;
                return;
            }

            ApplicationState = ApplicationState.LoggedOut;

            _splashScreen.BeginInvoke(new Action(ScheduleMenuDataBind));
        }

        public void LogoutCompleted() {
            _topics.Clear();
            _invites.Clear();
            ApplicationState = ApplicationState.LoggedOut;
            _splashScreen.BeginInvoke(new Action(ScheduleMenuDataBind));
        }

        private void SetIcon(Icon icon)
        {
            if (!_exiting) _notifyIcon.Icon = icon;
        }

    }
}
