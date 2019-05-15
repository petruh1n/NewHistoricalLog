using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;

namespace NewHistoricalLog
{
	public class SApp
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
				SingleInstanceManager manager = new SingleInstanceManager();
				manager.Run(args);
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("{0}", ex.Message));
			}

		}
	}
	public class SingleInstanceManager : WindowsFormsApplicationBase
	{
		SingleInstanceApplication app;
		Logger logger = LogManager.GetCurrentClassLogger();

		public SingleInstanceManager()
		{
			this.IsSingleInstance = true;
		}

		protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
		{
			try
			{
				// First time app is launched
				app = new SingleInstanceApplication();
				app.Deactivated += App_Deactivated;
				app.Run();
				return false;
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Ошибка инициализации первого экземпляра приложения: {0}", ex.Message));
				return false;
			}
		}

		private void App_Deactivated(object sender, EventArgs e)
        {
            foreach (Window window in app.Windows)
            {
                if (window != app.MainWindow & !(window is HiddenPrintWindow))
                {
                    window.Close();
                }
            }
            app.MainWindow.WindowState = WindowState.Minimized;
        }

		protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
		{
			try
			{
				// Subsequent launches
				base.OnStartupNextInstance(eventArgs);
                bool flag = false;
                if (eventArgs.CommandLine != null)
                {
                    try
                    {
                        for (int i = 0; i < eventArgs.CommandLine.Count; i++)
                        {
                            if (eventArgs.CommandLine[i].ToUpper().Contains("MONITOR"))
                            {
                                Service.Monitor = Convert.ToInt32(eventArgs.CommandLine[i].Remove(0, eventArgs.CommandLine[i].IndexOf("_") + 1));
                            }
                            if(eventArgs.CommandLine[i].ToUpper().Contains("USERGROUP"))
                            {
                                if (eventArgs.CommandLine[i].Remove(0, Environment.GetCommandLineArgs()[i].IndexOf("_") + 1).ToUpper() == "ADMIN")
                                {
                                    Service.IsAdminMode = true;
                                }
                                else
                                {
                                    Service.IsAdminMode = false;
                                }
                                flag = true;
                            }
                        }
                        if (!flag)
                            Service.IsAdminMode = false;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(String.Format("Ошибка при чтении ключей: {0}", ex.Message));
                        Service.Monitor = 0;
                        Service.IsAdminMode = false;
                    }
                }
                app.MainWindow.Show();
                app.MainWindow.WindowState = WindowState.Normal;

            }
			catch (Exception ex)
			{
				logger.Error(String.Format("Ошибка чтения ключей второго экземпляра приложения: {0}", ex.Message));
			}
            app.Activate();
		}
	}
	public class SingleInstanceApplication : Application
	{
		Logger logger = LogManager.GetCurrentClassLogger();
		protected override void OnStartup(System.Windows.StartupEventArgs e)
		{
			try
			{
				base.OnStartup(e);
				// Create and show the application's main window
				MainWindow window = new MainWindow();
				window.Show();
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Ошибка старта первого эеземпляра приложения: {0}", ex.Message));
                Environment.Exit(0);
			}
		}

		public void Activate()
		{
			try
			{
				// Reactivate application's main window
				//this.MainWindow.Show();
				this.MainWindow.Activate();
                this.MainWindow.WindowState = WindowState.Normal;
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Ошибка активации экземпляра приложения: {0}", ex.Message));
			}
		}
	}
}
