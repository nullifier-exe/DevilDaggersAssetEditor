﻿using DevilDaggersAssetCore;
using DevilDaggersAssetCore.ModFiles;
using DevilDaggersAssetCore.User;
using DevilDaggersAssetEditor.Code;
using DevilDaggersAssetEditor.Code.FileTabControlHandlers;
using DevilDaggersCore.Tools;
using DevilDaggersCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace DevilDaggersAssetEditor.Gui.Windows
{
	public partial class MainWindow : Window
	{
		private readonly List<Point> tabControlSizes = new List<Point>
		{
			new Point(3840, 2160),
			new Point(2560, 1440),
			new Point(2048, 1152),
			new Point(1920, 1200),
			new Point(1920, 1080),
			new Point(1680, 1050),
			new Point(1440, 900),
			new Point(1366, 768)
		};

		public Point CurrentTabControlSize { get; private set; }

		private UserCache Cache => UserHandler.Instance.cache;

		public MainWindow()
		{
			UserHandler.Instance.LoadSettings();
			UserHandler.Instance.LoadCache();

			InitializeComponent();

			TabControl.SelectedIndex = MathUtils.Clamp(Cache.ActiveTabIndex, 0, 6);
			if (Cache.WindowWidth > 128)
				Width = Cache.WindowWidth;
			if (Cache.WindowHeight > 128)
				Height = Cache.WindowHeight;
			if (Cache.WindowIsFullScreen)
				WindowState = WindowState.Maximized;

			App.Instance.MainWindow = this;
			App.Instance.UpdateMainWindowTitle();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (object senderCheckVersion, DoWorkEventArgs eCheckVersion) =>
			{
				VersionHandler.Instance.GetOnlineVersion(App.ApplicationName, App.LocalVersion);
			};
			worker.RunWorkerCompleted += (object senderCheckVersion, RunWorkerCompletedEventArgs eCheckVersion) =>
			{
				VersionResult versionResult = VersionHandler.Instance.VersionResult;

				if (versionResult.Exception != null)
					App.Instance.ShowError($"Error retrieving version number for '{App.ApplicationName}'", versionResult.Exception.Message, versionResult.Exception.InnerException);

				if (versionResult.IsUpToDate.HasValue && !versionResult.IsUpToDate.Value)
				{
					UpdateRecommendedWindow updateRecommendedWindow = new UpdateRecommendedWindow();
					updateRecommendedWindow.ShowDialog();
				}
			};
			worker.RunWorkerAsync();

			// After the window has loaded, some user controls still need to finish loading, so set a timer to make sure everything has loaded.
			// TODO: Find a better way to do this.
			DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 10) };
			timer.Tick += (sender, e) =>
			{
				AutoLoadMod(Cache.OpenedAudioModFilePath, BinaryFileType.Audio);
				AutoLoadMod(Cache.OpenedCoreModFilePath, BinaryFileType.Core);
				AutoLoadMod(Cache.OpenedDdModFilePath, BinaryFileType.Dd);
				AutoLoadMod(Cache.OpenedParticleModFilePath, BinaryFileType.Particle);

				void AutoLoadMod(string path, BinaryFileType binaryFileType)
				{
					if (File.Exists(path))
					{
						ModFile modFile = ModHandler.Instance.GetModFileFromPath(path, binaryFileType);
						if (modFile != null)
							foreach (AbstractFileTabControlHandler tabHandler in MenuBar.tabHandlers.Where(t => t.FileHandler.BinaryFileType == binaryFileType))
								tabHandler.UpdateAssetTabControls(modFile.Assets);
					}
				}
				timer.Stop();
			};
			timer.Start();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			for (int i = 0; i < tabControlSizes.Count; i++)
			{
				Point size = tabControlSizes[i];
				if (i == tabControlSizes.Count - 1 || ActualWidth >= size.X && ActualHeight >= size.Y - 24)
				{
					CurrentTabControlSize = size;
					TabControl.Width = size.X - 17;
					TabControl.Height = size.Y - 81;
					break;
				}
			}
		}
	}
}