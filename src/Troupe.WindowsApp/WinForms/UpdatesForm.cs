using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using AppLimit.NetSparkle;

namespace Troupe.WindowsApp.WinForms
{
    public partial class UpdatesForm : Form {
        private readonly Sparkle _sparkle;
        private string latestVersion = null;
        private bool exiting = false;
        public UpdatesForm(Sparkle sparkle)
        {
            InitializeComponent();
            _sparkle = sparkle;
            _sparkle.checkLoopStarted += new LoopStartedOperation(_sparkle_checkLoopStarted);
            _sparkle.checkLoopFinished += new LoopFinishedOperation(_sparkle_checkLoopFinished);
            _sparkle.updateDetected += new UpdateDetected(_sparkle_updateDetected);
            labelInstalledVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
            labelLatestVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
        }

        public void Kill() {
            exiting = true;
        }

        void _sparkle_updateDetected(object sender, UpdateDetectedEventArgs e) {
            latestVersion = e.LatestVersion.Version;

            if (!Visible) return;

            this.BeginInvoke(new Action(() => {
                                            this.Text = "update detected...";
                                            labelLatestVersion.Text = e.LatestVersion.Version;
                                        }
                                 ));
        }

        void _sparkle_checkLoopFinished(object sender, bool UpdateRequired) {
            if (!Visible) return;
            this.BeginInvoke(new Action(() => {
                                            if (!UpdateRequired) {
                                                labelLatestVersion.Text = labelInstalledVersion.Text;
                                            }
                                        }
                                 ));
        }

        void _sparkle_checkLoopStarted(object sender) {
            if (!Visible) return;

            this.BeginInvoke(new Action(() => {
                                            labelLatestVersion.Text = "..." + latestVersion;
                                        }
                                 ));
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void UpdatesForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (!exiting) {
                e.Cancel = true;
                Hide();
            }
        }

        private void UpdatesForm_Shown(object sender, EventArgs e) {
            labelLatestVersion.Text = latestVersion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _sparkle.Reset();
        }
    }
}
