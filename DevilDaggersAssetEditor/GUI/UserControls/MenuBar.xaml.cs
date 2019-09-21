﻿using DevilDaggersAssetEditor.Code;
using DevilDaggersAssetEditor.Code.Web;
using DevilDaggersAssetEditor.GUI.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using DevilDaggersAssetEditor.Code.TabControlHandlers;
using Microsoft.Win32;
using Newtonsoft.Json;
using DevilDaggersAssetCore;
using DevilDaggersAssetCore.ModFiles;
using DevilDaggersAssetEditor.Code.User;

namespace DevilDaggersAssetEditor.GUI.UserControls
{
	public partial class MenuBarUserControl : UserControl
	{
		private readonly List<AbstractTabControlHandler> tabHandlers;

		public MenuBarUserControl()
		{
			InitializeComponent();

			tabHandlers = new List<AbstractTabControlHandler>();
			foreach (Type type in App.Instance.Assembly.GetTypes().Where(t => t.BaseType == typeof(AbstractTabControlHandler) && !t.IsAbstract).OrderBy(t => t.Name))
				tabHandlers.Add((AbstractTabControlHandler)Activator.CreateInstance(type));

			foreach (AbstractTabControlHandler tabHandler in tabHandlers)
				FileMenuItem.Items.Add(tabHandler.CreateFileTypeMenuItem());

#if DEBUG
			MenuItem testException = new MenuItem { Header = "Test Exception", Background = new SolidColorBrush(Color.FromRgb(0, 255, 64)) };
			testException.Click += TestException_Click;

			MenuItem debug = new MenuItem { Header = "Debug", Background = new SolidColorBrush(Color.FromRgb(0, 255, 64)) };
			debug.Items.Add(testException);

			MenuPanel.Items.Add(debug);
#endif
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new SettingsWindow();
			if (settingsWindow.ShowDialog() == true)
				using (StreamWriter sw = new StreamWriter(File.Create(UserSettings.FileName)))
					sw.Write(JsonConvert.SerializeObject(UserHandler.Instance.settings, Formatting.Indented));
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow();
			aboutWindow.ShowDialog();
		}

		private void SourceCode_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(UrlUtils.SourceCode);
		}

		private void Update_Click(object sender, RoutedEventArgs e)
		{
			CheckingForUpdatesWindow window = new CheckingForUpdatesWindow();
			window.ShowDialog();

			if (NetworkHandler.Instance.VersionResult.IsUpToDate.HasValue)
			{
				if (!NetworkHandler.Instance.VersionResult.IsUpToDate.Value)
				{
					UpdateRecommendedWindow updateRecommendedWindow = new UpdateRecommendedWindow();
					updateRecommendedWindow.ShowDialog();
				}
				else
				{
					App.Instance.ShowMessage("Up to date", $"{ApplicationUtils.ApplicationDisplayNameWithVersion} is up to date.");
				}
			}
		}

		private void ShowLog_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists("DDAE.log"))
				Process.Start("DDAE.log");
			else
				App.Instance.ShowMessage("No log file", "Log file does not exist.");
		}

		private void TestException_Click(object sender, RoutedEventArgs e)
		{
			throw new Exception("Test Exception");
		}

		private void ConvertModFile0200_Click(object sender, RoutedEventArgs e)
		{
			string modFileFilter = "Audio mod files (*.audio)|*.audio";
			OpenFileDialog openDialog = new OpenFileDialog { InitialDirectory = UserHandler.Instance.settings.ModsRootFolder, Filter = modFileFilter };
			bool? openResult = openDialog.ShowDialog();
			if (!openResult.HasValue || !openResult.Value)
				return;
			string oldFileContents = File.ReadAllText(openDialog.FileName);

			// Fix namespace.
			string newFileContents = oldFileContents.Replace("Assets.UserAssets", "ModFiles");

			// Audio was the only asset type to edit in 0.2.0.0.
			List<AudioUserAsset> assets = JsonConvert.DeserializeObject<List<AudioUserAsset>>(newFileContents);

			JsonUtils.SerializeToFile(openDialog.FileName, new ModFile(ApplicationUtils.ApplicationVersionNumber, false, assets.Cast<GenericUserAsset>().ToList()), true, Formatting.None);
		}
	}
}