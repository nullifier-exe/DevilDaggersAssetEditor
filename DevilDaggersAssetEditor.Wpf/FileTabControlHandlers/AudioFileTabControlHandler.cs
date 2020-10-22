﻿using DevilDaggersAssetEditor.Assets;
using DevilDaggersAssetEditor.BinaryFileHandlers;
using DevilDaggersAssetEditor.ModFiles;
using DevilDaggersAssetEditor.Wpf.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Wpf.FileTabControlHandlers
{
	public class AudioFileTabControlHandler : AbstractFileTabControlHandler
	{
		public override AbstractBinaryFileHandler FileHandler => new ResourceFileHandler(BinaryFileType.Audio);

		public override MenuItem CreateFileTypeMenuItem()
		{
			MenuItem fileTypeMenuItem = base.CreateFileTypeMenuItem();

			MenuItem audioImport = new MenuItem { Header = $"Import Audio paths from folder" };
			MenuItem loudnessImport = new MenuItem { Header = $"Import Loudness from file" };
			MenuItem loudnessExport = new MenuItem { Header = $"Export Loudness to file" };

			audioImport.Click += (sender, e) => App.Instance.MainWindow!.AudioAudioAssetTabControl.ImportFolder();
			loudnessImport.Click += (sender, e) => LoudnessImportExport.ImportLoudness(App.Instance.MainWindow!.AudioAudioAssetTabControl.RowHandlers);
			loudnessExport.Click += (sender, e) => LoudnessImportExport.ExportLoudness(App.Instance.MainWindow!.AudioAudioAssetTabControl.RowHandlers);

			fileTypeMenuItem.Items.Add(audioImport);
			fileTypeMenuItem.Items.Add(new Separator());
			fileTypeMenuItem.Items.Add(loudnessImport);
			fileTypeMenuItem.Items.Add(loudnessExport);

			return fileTypeMenuItem;
		}

		public override List<AbstractAsset> GetAssets()
			=> App.Instance.MainWindow!.AudioAudioAssetTabControl.RowHandlers.Select(a => a.Asset).ToList();

		public override void UpdateAssetTabControls(List<UserAsset> assets)
			=> UpdateAssetTabControl(assets.OfType<AudioUserAsset>().ToList(), App.Instance.MainWindow!.AudioAudioAssetTabControl);

		protected override bool IsComplete()
			=> App.Instance.MainWindow!.AudioAudioAssetTabControl.IsComplete();
	}
}