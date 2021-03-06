﻿using DevilDaggersAssetEditor.User;
using DevilDaggersAssetEditor.Wpf.Network;
using DevilDaggersCore.Wpf.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevilDaggersAssetEditor.Wpf.Gui.Windows
{
	public partial class LoadingWindow : Window
	{
		private int _threadsComplete;
		private readonly List<BackgroundWorker> _threads = new List<BackgroundWorker>();
		private readonly List<string> _threadMessages = new List<string>();

		public LoadingWindow()
		{
			InitializeComponent();

			VersionLabel.Content = $"Version {App.LocalVersion}";

#if DEBUG
			VersionLabel.Background = ColorUtils.ThemeColors["SuccessText"];
			VersionLabel.Content += " DEBUG";
#endif

			Loaded += RunThreads;
		}

		private void RunThreads(object? sender, EventArgs e)
		{
			using BackgroundWorker checkVersionThread = new BackgroundWorker();
			checkVersionThread.DoWork += (sender, e) => NetworkHandler.Instance.GetOnlineTool();
			checkVersionThread.RunWorkerCompleted += (sender, e) =>
			{
				Dispatcher.Invoke(() =>
				{
					string message = string.Empty;
					SolidColorBrush color;

					if (NetworkHandler.Instance.Tool == null)
					{
						message = "Error";
						color = ColorUtils.ThemeColors["ErrorText"];
					}
					else
					{
						if (App.LocalVersion < Version.Parse(NetworkHandler.Instance.Tool.VersionNumberRequired))
						{
							message = "Warning (update required)";
							color = ColorUtils.ThemeColors["WarningText"];
						}
						else if (App.LocalVersion < Version.Parse(NetworkHandler.Instance.Tool.VersionNumber))
						{
							message = "Warning (update recommended)";
							color = ColorUtils.ThemeColors["SuggestionText"];
						}
						else
						{
							message = "OK (up to date)";
							color = ColorUtils.ThemeColors["SuccessText"];
						}
					}

					TaskResultsStackPanel.Children.Add(new Label
					{
						Content = message,
						Foreground = color,
						FontWeight = FontWeights.Bold,
					});
				});

				ThreadComplete();
			};

			bool readUserSettingsSuccess = false;
			bool userSettingsFileExists = File.Exists(UserSettings.FileName);
			using BackgroundWorker readUserSettingsThread = new BackgroundWorker();
			readUserSettingsThread.DoWork += (sender, e) =>
			{
				try
				{
					if (userSettingsFileExists)
					{
						using StreamReader sr = new StreamReader(File.OpenRead(UserSettings.FileName));
						UserHandler.Instance.Settings = JsonConvert.DeserializeObject<UserSettings>(sr.ReadToEnd());
					}

					readUserSettingsSuccess = true;
				}
				catch (Exception ex)
				{
					App.Instance.ShowError("Error", "Error while trying to read user settings.", ex);
				}
			};
			readUserSettingsThread.RunWorkerCompleted += (sender, e) =>
			{
				Dispatcher.Invoke(() =>
				{
					TaskResultsStackPanel.Children.Add(new Label
					{
						Content = readUserSettingsSuccess ? userSettingsFileExists ? "OK (found user settings)" : "OK (created new user settings)" : "Error",
						Foreground = readUserSettingsSuccess ? ColorUtils.ThemeColors["SuccessText"] : ColorUtils.ThemeColors["ErrorText"],
						FontWeight = FontWeights.Bold,
					});
				});

				ThreadComplete();
			};

			bool readUserCacheSuccess = false;
			bool userCacheFileExists = File.Exists(UserCache.FileName);
			using BackgroundWorker readUserCacheThread = new BackgroundWorker();
			readUserCacheThread.DoWork += (sender, e) =>
			{
				try
				{
					if (userCacheFileExists)
					{
						using StreamReader sr = new StreamReader(File.OpenRead(UserCache.FileName));
						UserHandler.Instance.Cache = JsonConvert.DeserializeObject<UserCache>(sr.ReadToEnd());
					}

					readUserCacheSuccess = true;
				}
				catch (Exception ex)
				{
					App.Instance.ShowError("Error", "Error while trying to read user cache.", ex);
				}
			};
			readUserCacheThread.RunWorkerCompleted += (sender, e) =>
			{
				Dispatcher.Invoke(() =>
				{
					TaskResultsStackPanel.Children.Add(new Label
					{
						Content = readUserCacheSuccess ? userCacheFileExists ? "OK (found user cache)" : "OK (created new user cache)" : "Error",
						Foreground = readUserCacheSuccess ? ColorUtils.ThemeColors["SuccessText"] : ColorUtils.ThemeColors["ErrorText"],
						FontWeight = FontWeights.Bold,
					});
				});

				ThreadComplete();
			};

			using BackgroundWorker mainInitThread = new BackgroundWorker();
			mainInitThread.DoWork += (sender, e) =>
			{
				Dispatcher.Invoke(() =>
				{
					MainWindow mainWindow = new MainWindow();
					mainWindow.Show();
				});
			};
			mainInitThread.RunWorkerCompleted += (sender, e) => Close();

			_threads.Add(checkVersionThread);
			_threads.Add(readUserSettingsThread);
			_threads.Add(readUserCacheThread);
			_threads.Add(mainInitThread);

			_threadMessages.Add("Checking for updates...");
			_threadMessages.Add("Reading user settings...");
			_threadMessages.Add("Reading user cache...");
			_threadMessages.Add("Initializing application...");

			RunThread(_threads[0]);
		}

		private void ThreadComplete()
		{
			_threadsComplete++;

			RunThread(_threads[_threadsComplete]);
		}

		private void RunThread(BackgroundWorker worker)
		{
			TasksStackPanel.Children.Add(new Label
			{
				Content = _threadMessages[_threadsComplete],
			});

			worker.RunWorkerAsync();
		}
	}
}
