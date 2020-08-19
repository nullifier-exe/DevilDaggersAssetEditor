﻿using DevilDaggersAssetCore;
using DevilDaggersAssetCore.User;
using DevilDaggersAssetEditor.Gui.Windows;
using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DevilDaggersAssetEditor
{
	public partial class App : Application
	{
		public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType ?? throw new Exception("Could not get declaring type of current method."));

		public App()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			Utils.GuiVersion = LocalVersion;

			Dispatcher.UnhandledException += OnDispatcherUnhandledException;

			ILoggerRepository? logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
			XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
		}

		public static string ApplicationName => "DevilDaggersAssetEditor";
		public static string ApplicationDisplayName => "Devil Daggers Asset Editor";

		public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
		public static Version LocalVersion { get; } = Version.Parse(FileVersionInfo.GetVersionInfo(Assembly.Location).FileVersion);

		public static App Instance => (App)Current;
		public new MainWindow MainWindow { get; set; }

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			ShowError("Fatal error", "An unhandled exception occurred in the main thread.", e.Exception);
			e.Handled = true;

			Current.Shutdown();
		}

		public void UpdateMainWindowTitle()
		{
			Dispatcher.Invoke(() =>
			{
				MainWindow.Title = $"{ApplicationDisplayName} {LocalVersion}";
			});
		}

		/// <summary>
		/// Logs the error message (and <see cref="Exception" /> if there is one).
		/// </summary>
		public static void LogError(string message, Exception? ex)
		{
			if (ex != null)
				Log.Error(message, ex);
			else
				Log.Error(message);
		}

		/// <summary>
		/// Shows the error using the <see cref="ErrorWindow" /> and then calls <see cref="LogError(string, Exception)" /> to log the error message (and <see cref="Exception" /> if there is one).
		/// </summary>
		public void ShowError(string title, string message, Exception? ex = null)
		{
			LogError(message, ex);

			Dispatcher.Invoke(() =>
			{
				ErrorWindow errorWindow = new ErrorWindow(title, message, ex);
				errorWindow.ShowDialog();
			});
		}

		public void ShowMessage(string title, string message)
		{
			Dispatcher.Invoke(() =>
			{
				MessageWindow messageWindow = new MessageWindow(title, message);
				messageWindow.ShowDialog();
			});
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			UserHandler.Instance.cache.ActiveTabIndex = MainWindow.TabControl.SelectedIndex;
			UserHandler.Instance.cache.WindowWidth = (int)MainWindow.Width;
			UserHandler.Instance.cache.WindowHeight = (int)MainWindow.Height;
			UserHandler.Instance.cache.WindowIsFullScreen = MainWindow.WindowState == WindowState.Maximized;
			UserHandler.Instance.SaveCache();
		}
	}
}