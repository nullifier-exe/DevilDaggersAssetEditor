﻿using DevilDaggersAssetCore;
using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetCore.BinaryFileHandlers;
using DevilDaggersAssetCore.ModFiles;
using DevilDaggersAssetCore.User;
using DevilDaggersAssetEditor.Code.RowControlHandlers;
using DevilDaggersAssetEditor.Code.TabControlHandlers;
using DevilDaggersAssetEditor.Gui.Windows;
using JsonUtils;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Code.FileTabControlHandlers
{
	public abstract class AbstractFileTabControlHandler
	{
		public abstract AbstractBinaryFileHandler FileHandler { get; }

		private UserSettings Settings => UserHandler.Instance.settings;

		public virtual MenuItem CreateFileTypeMenuItem()
		{
			BinaryFileType binaryFileType = FileHandler.BinaryFileType;
			string fileName = binaryFileType.ToString().ToLower();

			MenuItem extractItem = new MenuItem { Header = $"Extract '{fileName}'" };
			MenuItem compressItem = new MenuItem { Header = $"Compress '{fileName}'" };
			MenuItem openModFileItem = new MenuItem { Header = $"Open .{fileName} mod file" };
			MenuItem saveModFileItem = new MenuItem { Header = $"Save .{fileName} mod file" };

			extractItem.Click += (sender, e) => Extract_Click();
			compressItem.Click += (sender, e) => Compress_Click();
			openModFileItem.Click += (sender, e) =>
			{
				ModFile modFile = OpenModFile();
				if (modFile == null)
					return;
				UpdateAssetTabControls(modFile.Assets);
			};
			saveModFileItem.Click += (sender, e) =>
			{
				List<AbstractAsset> assets = GetAssets();
				List<AbstractUserAsset> userAssets = CreateUserAssets(assets);
				SaveModFile(userAssets);
			};

			MenuItem fileTypeMenuItem = new MenuItem { Header = fileName };

			fileTypeMenuItem.Items.Add(extractItem);
			fileTypeMenuItem.Items.Add(compressItem);
			fileTypeMenuItem.Items.Add(new Separator());
			fileTypeMenuItem.Items.Add(openModFileItem);
			fileTypeMenuItem.Items.Add(saveModFileItem);
			fileTypeMenuItem.Items.Add(new Separator());

			return fileTypeMenuItem;
		}

		protected abstract bool IsComplete();

		public abstract List<AbstractAsset> GetAssets();

		private async void Extract_Click()
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			if (Settings.EnableDevilDaggersRootFolder)
				openDialog.InitialDirectory = Path.Combine(Settings.DevilDaggersRootFolder, FileHandler.BinaryFileType.GetSubfolderName());

			bool? openResult = openDialog.ShowDialog();
			if (!openResult.HasValue || !openResult.Value)
				return;

			using CommonOpenFileDialog folderDialog = new CommonOpenFileDialog { IsFolderPicker = true };
			if (Settings.EnableModsRootFolder)
				folderDialog.InitialDirectory = Settings.ModsRootFolder;

			if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				ProgressWindow progressWindow = new ProgressWindow($"Extracting '{FileHandler.BinaryFileType.ToString().ToLower()}'...");
				progressWindow.Show();
				await Task.Run(() =>
				{
					FileHandler.Extract(
						openDialog.FileName,
						folderDialog.FileName,
						FileHandler.BinaryFileType,
						new Progress<float>(value => App.Instance.Dispatcher.Invoke(() => progressWindow.ProgressBar.Value = value)),
						new Progress<string>(value => App.Instance.Dispatcher.Invoke(() => progressWindow.ProgressDescription.Text = value)));
					App.Instance.Dispatcher.Invoke(() => progressWindow.Finish());
				});
			}
		}

		private async void Compress_Click()
		{
			if (!IsComplete())
			{
				MessageBoxResult promptResult = MessageBox.Show("Not all file paths have been specified. In most cases this will cause Devil Daggers to crash on start up (or sometimes randomly at runtime). Are you sure you wish to continue?", "Incomplete asset list", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (promptResult == MessageBoxResult.No)
					return;
			}

			SaveFileDialog dialog = new SaveFileDialog();
			if (Settings.EnableDevilDaggersRootFolder)
				dialog.InitialDirectory = Path.Combine(Settings.DevilDaggersRootFolder, FileHandler.BinaryFileType.GetSubfolderName());

			bool? result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value)
				return;

			ProgressWindow progressWindow = new ProgressWindow($"Compressing '{FileHandler.BinaryFileType.ToString().ToLower()}'...");
			progressWindow.Show();
			await Task.Run(() =>
			{
				try
				{
					FileHandler.Compress(
						allAssets: GetAssets(),
						outputPath: dialog.FileName,
						progress: new Progress<float>(value => App.Instance.Dispatcher.Invoke(() => progressWindow.ProgressBar.Value = value)),
						progressDescription: new Progress<string>(value => App.Instance.Dispatcher.Invoke(() => progressWindow.ProgressDescription.Text = value)));

					App.Instance.Dispatcher.Invoke(() => progressWindow.Finish());
				}
				catch (Exception ex)
				{
					App.Instance.Dispatcher.Invoke(() =>
					{
						App.Instance.ShowError("Compression failed", $"Compressing assets failed during the execution of \"{progressWindow.ProgressDescription.Text}\".", ex);
						progressWindow.Error();
					});
				}
			});
		}

		private void SaveModFile(List<AbstractUserAsset> assets)
		{
			string modFileExtension = FileHandler.BinaryFileType.ToString().ToLower();
			string modFileFilter = $"{FileHandler.BinaryFileType} mod files (*.{modFileExtension})|*.{modFileExtension}";
			SaveFileDialog dialog = new SaveFileDialog { Filter = modFileFilter };
			if (Settings.EnableModsRootFolder)
				dialog.InitialDirectory = Settings.ModsRootFolder;

			bool? result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value)
				return;

			bool relativePaths = false;
			if (AssetsHaveSameBasePaths())
			{
				MessageBoxResult relativePathsResult = MessageBox.Show("Specify whether you want this mod file to use relative paths (easier to share between computers or using zipped files containing assets).", "Use relative paths?", MessageBoxButton.YesNo, MessageBoxImage.Question);

				if (relativePathsResult == MessageBoxResult.Yes)
					foreach (AbstractUserAsset asset in assets)
						asset.EditorPath = Path.GetFileName(asset.EditorPath);
			}
			ModFile modFile = new ModFile(App.LocalVersion, relativePaths, assets);

			JsonFileUtils.SerializeToFile(dialog.FileName, modFile, true);

			bool AssetsHaveSameBasePaths()
			{
				List<AbstractUserAsset> assetList = assets.ToList();
				for (int i = 0; i < assetList.Count; i++)
				{
					string path1 = assetList[i].EditorPath;
					string path2 = assetList[(i + 1) % assetList.Count].EditorPath;
					if (!File.Exists(path1) || !File.Exists(path2) || Path.GetDirectoryName(path1) != Path.GetDirectoryName(path2))
						return false;
				}

				return true;
			}
		}

		private List<AbstractUserAsset> CreateUserAssets(List<AbstractAsset> assets)
		{
			List<AbstractUserAsset> userAssets = new List<AbstractUserAsset>();
			foreach (AbstractAsset asset in assets)
				userAssets.Add(asset.ToUserAsset());
			return userAssets;
		}

		private ModFile OpenModFile()
		{
			string modFileExtension = FileHandler.BinaryFileType.ToString().ToLower();
			string modFileFilter = $"{FileHandler.BinaryFileType} mod files (*.{modFileExtension})|*.{modFileExtension}";
			OpenFileDialog dialog = new OpenFileDialog { Filter = modFileFilter };
			if (Settings.EnableModsRootFolder)
				dialog.InitialDirectory = Settings.ModsRootFolder;

			bool? openResult = dialog.ShowDialog();
			if (!openResult.HasValue || !openResult.Value)
				return null;

			return ModHandler.Instance.GetModFileFromPath(dialog.FileName, FileHandler.BinaryFileType);
		}

		public abstract void UpdateAssetTabControls(List<AbstractUserAsset> assets);

		protected void UpdateAssetTabControl<TUserAsset, TAsset, TAssetRowControl, TAssetRowControlHandler>(List<TUserAsset> userAssets, AbstractAssetTabControlHandler<TAsset, TAssetRowControl, TAssetRowControlHandler> assetTabControlHandler)
			where TUserAsset : AbstractUserAsset
			where TAsset : AbstractAsset
			where TAssetRowControl : UserControl
			where TAssetRowControlHandler : AbstractAssetRowControlHandler<TAsset, TAssetRowControl>
		{
			for (int i = 0; i < assetTabControlHandler.AssetRowEntries.Count; i++)
			{
				TAsset asset = assetTabControlHandler.AssetRowEntries[i].AssetRowControlHandler.Asset;
				TUserAsset userAsset = userAssets.FirstOrDefault(a => a.AssetName == asset.AssetName && a.ChunkTypeName == asset.ChunkTypeName);
				if (userAsset != null)
				{
					asset.ImportValuesFromUserAsset(userAsset);

					assetTabControlHandler.UpdateGui(asset);
				}
			}
		}
	}
}