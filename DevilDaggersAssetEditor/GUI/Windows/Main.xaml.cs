﻿using DevilDaggersAssetCore;
using DevilDaggersAssetEditor.Code.Assets;
using DevilDaggersAssetEditor.GUI.UserControls;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.GUI.Windows
{
	public partial class MainWindow : Window
	{
		private static readonly string DDFolder = @"C:\Program Files (x86)\Steam\steamapps\common\devildaggers";

		public MainWindow()
		{
			InitializeComponent();

			foreach (BinaryFileName binaryFileName in (BinaryFileName[])Enum.GetValues(typeof(BinaryFileName)))
			{
				MenuItem extractItem = new MenuItem { Header = binaryFileName.ToString() };
				MenuItem compressItem = new MenuItem { Header = binaryFileName.ToString() };

				extractItem.Click += (sender, e) => Extract_Click(binaryFileName);
				compressItem.Click += (sender, e) => Compress_Click(binaryFileName);

				ExtractMenuItem.Items.Add(extractItem);
				CompressMenuItem.Items.Add(compressItem);
			}

			foreach (AudioAsset audioAsset in AssetHandler.Instance.AudioAssets)
			{
				AssetEditor.Children.Add(new AudioAssetControl(audioAsset));
			}
		}

		private void Extract_Click(BinaryFileName binaryFileName)
		{
			OpenFileDialog openDialog = new OpenFileDialog
			{
				InitialDirectory = Path.Combine(DDFolder, binaryFileName.GetSubfolderName())
			};
			bool? openResult = openDialog.ShowDialog();
			if (!openResult.HasValue || !openResult.Value)
				return;

			using (CommonOpenFileDialog saveDialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true
			})
			{
				CommonFileDialogResult saveResult = saveDialog.ShowDialog();
				if (saveResult == CommonFileDialogResult.Ok)
				{
					Extractor.Extract(openDialog.FileName, saveDialog.FileName, binaryFileName);
				}
			}
		}

		private void Compress_Click(BinaryFileName binaryFileName)
		{
			SaveFileDialog dialog = new SaveFileDialog
			{
				InitialDirectory = Path.Combine(DDFolder, binaryFileName.GetSubfolderName())
			};
			bool? result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value)
				return;

			bool complete = true;
			foreach (AudioAsset audioAsset in AssetHandler.Instance.AudioAssets)
			{
				if (audioAsset.EditorPath == null)
				{
					complete = false;
					break;
				}
			}

			if (!complete)
			{
				MessageBoxResult promptResult = MessageBox.Show("Not all files have been specified. In most cases this will cause Devil Daggers to crash on start up. Are you sure you wish to continue?", "Incomplete assets", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (promptResult == MessageBoxResult.No)
					return;
			}

			Compressor.Compress(AssetHandler.Instance.AudioAssets.Select(a => a.EditorPath).ToArray(), dialog.FileName, binaryFileName);
		}

		private void Import_Click(object sender, RoutedEventArgs e)
		{
			using (CommonOpenFileDialog dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true
			})
			{
				CommonFileDialogResult result = dialog.ShowDialog();
				if (result == CommonFileDialogResult.Ok)
				{

				}
			}
		}
	}
}