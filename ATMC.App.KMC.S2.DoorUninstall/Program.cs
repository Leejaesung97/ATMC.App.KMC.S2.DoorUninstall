using LotusAPI;
using System;
using System.Windows.Forms;
    
namespace ATMC.App.KMC.S2.DoorUninstall {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            //1. Allow only 1 instance
            if(ThreadUtils.IsAppRunning()) {
                DialogUtils.ShowErrMsg("App is running!");
                return;
            }

            //2. Register all unhandled exception
            AppUtils.RegisterUnhandledExceptionHandler();

            //C# stuff
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            // 3. Setup Registry app node
            Registry.SetApplicationName(Application.ProductName);

            //4. Create main form using splash screen
            var f = Abeo.Controls.Dialogs.SplashWindow.CreateForm_<FormMain>(null, Application.ProductName, true, 2000);
            Application.Run(f);
        }
    }
}
