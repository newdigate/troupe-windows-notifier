using System;
using System.Windows.Forms;
using Xilium.CefGlue;

namespace Troupe.WindowsApp.Auth.Chromium
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                CefRuntime.Load();
            }
            catch (DllNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            catch (CefRuntimeException ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 3;
            }

            var mainArgs = new CefMainArgs(args);
            var app = new DemoApp();

            var exitCode = CefRuntime.ExecuteProcess(mainArgs, app);
            if (exitCode != -1)
                return exitCode;

            var settings = new CefSettings
                {
                    SingleProcess = false,
                    MultiThreadedMessageLoop = true,
                    LogSeverity = CefLogSeverity.Disable,
                    LogFile = "CefGlue.log",
                };

            CefRuntime.Initialize(mainArgs, settings, app);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new LoginForm(null, "https://beta.trou.pe/oauth/login?client_id=5&redirect_uri=http%3A%2F%2Ftrou.pe%2FOAuthCallback&response_type=code&scope=read"));

            CefRuntime.Shutdown();
            return 0;
        }
    }
}
