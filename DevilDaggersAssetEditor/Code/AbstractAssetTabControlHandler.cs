﻿using DevilDaggersAssetCore;
using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetCore.Info;
using DevilDaggersAssetCore.User;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DevilDaggersAssetEditor.Code
{
	public abstract class AbstractAssetTabControlHandler<TAsset, TAssetRowControl>
		where TAsset : AbstractAsset
		where TAssetRowControl : UserControl
	{
		protected abstract string AssetTypeJsonFileName { get; }

		public TAsset SelectedAsset { get; set; }

		public List<AssetRowEntry<TAsset, TAssetRowControl>> AssetRowEntries { get; private set; } = new List<AssetRowEntry<TAsset, TAssetRowControl>>();

		private UserSettings settings => UserHandler.Instance.settings;

		public readonly List<StackPanel> filterStackPanels = new List<StackPanel>();
		public readonly List<CheckBox> filterCheckBoxes = new List<CheckBox>();

		private readonly Color filterHighlightColor;

		protected AbstractAssetTabControlHandler(BinaryFileType binaryFileType)
		{
			using StreamReader sr = new StreamReader(Utils.GetAssemblyByName("DevilDaggersAssetCore").GetManifestResourceStream($"DevilDaggersAssetCore.Content.{binaryFileType.ToString().ToLower()}.{AssetTypeJsonFileName}.json"));
			List<TAsset> assets = JsonConvert.DeserializeObject<List<TAsset>>(sr.ReadToEnd());

			int i = 0;
			foreach (TAsset asset in assets)
			{
				TAssetRowControl assetRowControl = (TAssetRowControl)Activator.CreateInstance(typeof(TAssetRowControl), asset, i++ % 2 == 0);
				AssetRowEntries.Add(new AssetRowEntry<TAsset, TAssetRowControl>(asset, assetRowControl, true));
			}

			ChunkInfo chunkInfo = ChunkInfo.All.FirstOrDefault(c => c.AssetType == typeof(TAsset));
			filterHighlightColor = chunkInfo.GetColor() * 0.25f;
		}

		public abstract void UpdateGui(TAsset asset);

		public void SelectAsset(TAsset asset)
		{
			SelectedAsset = asset;
		}

		public void ImportFolder()
		{
			using CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
			if (settings.EnableAssetsRootFolder)
				dialog.InitialDirectory = settings.AssetsRootFolder;

			CommonFileDialogResult result = dialog.ShowDialog();
			if (result != CommonFileDialogResult.Ok)
				return;

			foreach (string filePath in Directory.GetFiles(dialog.FileName))
			{
				TAsset asset = AssetRowEntries.Where(a => a.Asset.AssetName == Path.GetFileNameWithoutExtension(filePath).Replace("_fragment", "").Replace("_vertex", "")).Cast<TAsset>().FirstOrDefault();
				if (asset != null)
				{
					asset.EditorPath = filePath.Replace("_fragment", "").Replace("_vertex", "");
					UpdateGui(asset);
				}
			}
		}

		public bool IsComplete()
		{
			foreach (TAsset asset in AssetRowEntries.Select(a => a.Asset))
				if (!File.Exists(asset.EditorPath.Replace(".glsl", "_vertex.glsl")))
					return false;
			return true;
		}

		public void CreateFiltersGui()
		{
			IEnumerable<string> tags = AssetRowEntries.Select(a => a.Asset).SelectMany(a => a.Tags ?? (new string[] { })).Where(t => !string.IsNullOrEmpty(t)).Distinct().OrderBy(s => s);
			int filterColumnCount = 9;
			int i = 0;
			for (; i < filterColumnCount; i++)
			{
				StackPanel stackPanel = new StackPanel { Tag = i };
				Grid.SetColumn(stackPanel, i);
				filterStackPanels.Add(stackPanel);
			}

			i = 0;
			foreach (string tag in tags)
			{
				int pos = (int)(i++ / (float)tags.Count() * filterColumnCount);
				CheckBox checkBox = new CheckBox { Content = tag };
				filterCheckBoxes.Add(checkBox);
				filterStackPanels.FirstOrDefault(s => (int)s.Tag == pos).Children.Add(checkBox);
			}
		}

		public void ApplyFilter(FilterOperation filterOperation, Dictionary<TAssetRowControl, TextBlock> textBlocks)
		{
			IEnumerable<string> checkedFiters = filterCheckBoxes.Where(c => c.IsChecked.Value).Select(s => s.Content.ToString());

			foreach (AssetRowEntry<TAsset, TAssetRowControl> assetRowEntry in AssetRowEntries)
			{
				TextBlock textBlockTags = textBlocks.FirstOrDefault(t => t.Key == assetRowEntry.AssetRowControl).Value;

				if (checkedFiters.Count() == 0)
				{
					assetRowEntry.IsActive = true;

					textBlockTags.Text = string.Join(", ", assetRowEntry.Asset.Tags);
				}
				else
				{
					assetRowEntry.IsActive = filterOperation switch
					{
						FilterOperation.And => checkedFiters.All(t => assetRowEntry.Asset.Tags.Contains(t)),
						FilterOperation.Or => assetRowEntry.Asset.Tags.Any(t => checkedFiters.Contains(t)),
						_ => throw new NotImplementedException($"{nameof(FilterOperation)} {filterOperation} not implemented in {nameof(ApplyFilter)} method.")
					};
					if (!assetRowEntry.IsActive)
						continue;

					// If not collapsed, change TextBlock colors for found tags.
					textBlockTags.Inlines.Clear();

					string[] assetTags = assetRowEntry.Asset.Tags;
					for (int i = 0; i < assetTags.Length; i++)
					{
						string tag = assetTags[i];
						Run tagRun = new Run(tag);
						if (checkedFiters.Contains(tag))
							tagRun.Background = new SolidColorBrush(filterHighlightColor);
						textBlockTags.Inlines.Add(tagRun);
						if (i != assetTags.Length - 1)
							textBlockTags.Inlines.Add(new Run(", "));
					}
				}
			}
		}
	}
}