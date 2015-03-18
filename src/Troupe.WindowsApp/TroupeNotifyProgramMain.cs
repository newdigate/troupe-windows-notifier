using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Xilium.CefGlue;

namespace Troupe.WindowsApp {
    internal static class TroupeNotifyProgramMain {
        private static readonly ILog logger = LogManager.GetLogger(typeof (TroupeNotifyProgramMain));

        [STAThread]
        private static int Main(string[] args) {

            logger.Info("Main");
            bool alreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Length > 1;
            if (alreadyRunning)
                return 0;

            bool cefLoaded = false;

            try {
                try {
                    CefRuntime.Load();
                    cefLoaded = true;
                } catch (DllNotFoundException ex) {
                    MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                } catch (CefRuntimeException ex) {
                    MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 2;
                } catch (Exception ex) {
                    MessageBox.Show(ex.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 3;
                }

                CefMainArgs mainArgs =
                    new CefMainArgs(new[] {
                                              "--disable-gpu ",
                                              "--disable-plugins",
                                              "--disable-3d-apis",
                                              "--disable-audio"
                                          });
                DemoApp app = new DemoApp();

                int exitCode = CefRuntime.ExecuteProcess(mainArgs, app);
                if (exitCode != -1)
                    return -1;

                CefSettings settings = new CefSettings {
                                                           SingleProcess = true,
                                                           MultiThreadedMessageLoop = true,
                                                           LogSeverity = CefLogSeverity.Disable,
                                                           LogFile = "CefGlue.log",
                                                       };

                CefRuntime.Initialize(mainArgs, settings, app);

                if (!settings.MultiThreadedMessageLoop) {
                    Application.Idle += (sender, e) => { CefRuntime.DoMessageLoopWork(); };
                }
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new TroupeApplicationContext());

            } catch (Exception exc) {
                logger.Error(exc);
            } finally {
                if (cefLoaded)
                    CefRuntime.Shutdown();
            }
            return 0;
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            logger.Error(e.Exception);
        }
    }
}