#region

using System;
using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Zhwang.SuperNotifyIcon;

#endregion

namespace Troupe.WindowsApp.WinForms {
    public partial class BalloonForm : Form {

        private static readonly ILog logger = LogManager.GetLogger(typeof(BalloonForm));

        private readonly IScheduler _scheduler;
        private readonly SuperNotifyIcon _extensions;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public BalloonForm(SuperNotifyIcon extensions, IScheduler scheduler) {
            InitializeComponent();
            _extensions = extensions;
            _scheduler = scheduler;
        }

        public void ShowNotification(string title = "Notification", string detail = "Detail") {
            labelTitle.Text = title;
            labelDetail.Text = detail;
           
            UpdateBalloonLocation();
            Show();
        }

        public void SetTimeout(int milliseconds) {
            // obtain delegate to function pointer for Form.Close()
            Delegate d = new Action(Close);

            // after 6 seconds, call the close 
            Observable.Timer(TimeSpan.FromSeconds(6))
                .ObserveOn(_scheduler).Subscribe(l => {
                                               // close form on ui thread
                                               IAsyncResult r = BeginInvoke(d);
                                           }, _cancellationToken.Token);
        }

        public void UpdateBalloonLocation() {
            int x=0;
            int y=0;

            Point? p = _extensions.GetLocation();
            if (p.HasValue) {

                x = p.Value.X;
                y = p.Value.Y;

                if (y > Screen.PrimaryScreen.WorkingArea.Size.Height - this.Height)
                    y = Screen.PrimaryScreen.WorkingArea.Size.Height - this.Height;

                if (x > Screen.PrimaryScreen.WorkingArea.Size.Width - this.Width)
                    x = Screen.PrimaryScreen.WorkingArea.Size.Width - this.Width;
            }

            this.Left = x;
            this.Top = y;
        }

        public void ClearTimeout() {
            _cancellationToken.Cancel();
        }

        private void labelDetail_Click(object sender, EventArgs e)
        {
            InvokeOnClick(this, e);
        }

        private void BalloonForm_Load(object sender, EventArgs e)
        {

        }

        private void labelDetail_Click(object sender, MouseEventArgs e)
        {

        }
    }
}