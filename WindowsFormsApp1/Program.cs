using System;
using System.Threading;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace WindowsFormsApp1
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[Log(AttributeExclude = true)]
        [STAThread]
        static void Main()
        {
            //InitializeLoggingBackend();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.Run(new LogInForm());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Critical Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        //[Log(AttributeExclude = true)]
        //public static void InitializeLoggingBackend()
        //{
        //    log4net.Config.XmlConfigurator.Configure();
        //    var log4NetLoggingBackend = new Log4NetLoggingBackend();
        //    LoggingServices.DefaultBackend = log4NetLoggingBackend;
        //}
    }
}
