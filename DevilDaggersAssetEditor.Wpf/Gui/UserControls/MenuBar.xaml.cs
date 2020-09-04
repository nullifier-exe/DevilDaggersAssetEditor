﻿using DevilDaggersAssetEditor.BinaryFileAnalyzer;
using DevilDaggersAssetEditor.User;
using DevilDaggersAssetEditor.Wpf.Code.FileTabControlHandlers;
using DevilDaggersAssetEditor.Wpf.Code.Network;
using DevilDaggersAssetEditor.Wpf.Gui.Windows;
using DevilDaggersCore.Utils;
using DevilDaggersCore.Wpf.Models;
using DevilDaggersCore.Wpf.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
#if DEBUG
using System.Windows.Media;
#endif

namespace DevilDaggersAssetEditor.Wpf.Gui.UserControls
{
	public partial class MenuBarUserControl : UserControl
	{
		public MenuBarUserControl()
		{
			InitializeComponent();

			if (NetworkHandler.Instance.Tool != null && App.LocalVersion < Version.Parse(NetworkHandler.Instance.Tool.VersionNumber))
			{
				HelpItem.Header += " (Update available)";
				HelpItem.FontWeight = FontWeights.Bold;

				foreach (MenuItem? menuItem in HelpItem.Items)
				{
					if (menuItem == null)
						continue;
					menuItem.FontWeight = FontWeights.Normal;
				}

				UpdateItem.Header = "Update available";
				UpdateItem.FontWeight = FontWeights.Bold;
			}

			TabHandlers = App.Assembly
				.GetTypes()
				.Where(t => t.BaseType == typeof(AbstractFileTabControlHandler) && !t.IsAbstract)
				.OrderBy(t => t.Name)
				.Select(t => (AbstractFileTabControlHandler)Activator.CreateInstance(t))
				.ToList();

			foreach (AbstractFileTabControlHandler tabHandler in TabHandlers)
				FileMenuItem.Items.Add(tabHandler.CreateFileTypeMenuItem());

#if DEBUG
			MenuItem testException = new MenuItem { Header = "Test Exception", Background = new SolidColorBrush(Color.FromRgb(0, 255, 63)) };
			testException.Click += (sender, e) => throw new Exception("Test Exception");

			MenuItem debug = new MenuItem { Header = "Debug", Background = new SolidColorBrush(Color.FromRgb(0, 255, 63)) };
			debug.Items.Add(testException);

			MenuPanel.Items.Add(debug);
#endif
		}

		public List<AbstractFileTabControlHandler> TabHandlers { get; }

		private void AnalyzeBinaryFileMenuItem_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			if (UserHandler.Instance.Settings.EnableDevilDaggersRootFolder && Directory.Exists(UserHandler.Instance.Settings.DevilDaggersRootFolder))
				openDialog.InitialDirectory = UserHandler.Instance.Settings.DevilDaggersRootFolder;

			bool? openResult = openDialog.ShowDialog();
			if (openResult.HasValue && openResult.Value)
			{
				byte[] sourceFileBytes = File.ReadAllBytes(openDialog.FileName);

				AnalyzerFileResult? result = BinaryFileAnalyzerWindow.TryReadResourceFile(openDialog.FileName, sourceFileBytes);
				if (result == null)
					result = BinaryFileAnalyzerWindow.TryReadParticleFile(openDialog.FileName, sourceFileBytes);

				if (result == null)
				{
					App.Instance.ShowMessage("File not recognized", "Make sure to open one of the following binary files: audio, core, dd, particle");
				}
				else
				{
					BinaryFileAnalyzerWindow fileAnalyzerWindow = new BinaryFileAnalyzerWindow(result);
					fileAnalyzerWindow.ShowDialog();
				}
			}
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new SettingsWindow();
			if (settingsWindow.ShowDialog() == true)
				UserHandler.Instance.SaveSettings();
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow();
			aboutWindow.ShowDialog();
		}

		private void Changelog_Click(object sender, RoutedEventArgs e)
		{
			if (NetworkHandler.Instance.Tool != null)
			{
				List<ChangelogEntry> changes = NetworkHandler.Instance.Tool.Changelog.Select(c => new ChangelogEntry(Version.Parse(c.VersionNumber), c.Date, MapToSharedModel(c.Changes).ToList())).ToList();
				ChangelogWindow changelogWindow = new ChangelogWindow(changes, App.LocalVersion);
				changelogWindow.ShowDialog();
			}
			else
			{
				App.Instance.ShowError("Changelog not retrieved", "The changelog has not been retrieved from DevilDaggers.info.");
			}

			static IEnumerable<Change>? MapToSharedModel(List<Code.Clients.Change>? changes)
			{
				foreach (Code.Clients.Change change in changes ?? new List<Code.Clients.Change>())
					yield return new Change(change.Description, MapToSharedModel(change.SubChanges)?.ToList() ?? null);
			}
		}

		private void Help_Click(object sender, RoutedEventArgs e)
		{
			HelpWindow helpWindow = new HelpWindow();
			helpWindow.ShowDialog();
		}

		private void SourceCode_Click(object sender, RoutedEventArgs e)
			=> ProcessUtils.OpenUrl(UrlUtils.SourceCodeUrl(App.ApplicationName).ToString());

		private void Update_Click(object sender, RoutedEventArgs e)
		{
			CheckingForUpdatesWindow window = new CheckingForUpdatesWindow(NetworkHandler.Instance.GetOnlineTool);
			window.ShowDialog();

			if (NetworkHandler.Instance.Tool != null)
			{
				if (App.LocalVersion < Version.Parse(NetworkHandler.Instance.Tool.VersionNumber))
				{
					UpdateRecommendedWindow updateRecommendedWindow = new UpdateRecommendedWindow(NetworkHandler.Instance.Tool.VersionNumber, App.LocalVersion.ToString(), App.ApplicationName, App.ApplicationDisplayName);
					updateRecommendedWindow.ShowDialog();
				}
				else
				{
					App.Instance.ShowMessage("Up to date", $"{App.ApplicationDisplayName} {App.LocalVersion} is up to date.");
				}
			}
			else
			{
				App.Instance.ShowError("Error retrieving tool information", "An error occurred while attempting to retrieve tool information from the API.");
			}
		}

		private void ShowLog_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (File.Exists("DDAE.log"))
					Process.Start("DDAE.log");
				else
					App.Instance.ShowMessage("No log file", "Log file does not exist.");
			}
			catch (Exception ex)
			{
				App.Instance.ShowMessage("Could not open log file", ex.Message);
			}
		}
	}
}