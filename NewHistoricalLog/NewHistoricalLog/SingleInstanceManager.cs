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
			app.MainWindow.WindowState = WindowState.Minimized;
		}

		protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
		{
			try
			{
				// Subsequent launches
				base.OnStartupNextInstance(eventArgs);
				
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Ошибка чтения ключей второго экземпляра приложения: {0}", ex.Message));
			}
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
			}
		}

		public void Activate()
		{
			try
			{
				// Reactivate application's main window
				this.MainWindow.Show();
				this.MainWindow.Activate();
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Ошибка активации экземпляра приложения: {0}", ex.Message));
			}
		}
	}
}
