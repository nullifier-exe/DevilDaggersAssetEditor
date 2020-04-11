﻿using DevilDaggersAssetCore;
using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetEditor.Code;
using DevilDaggersAssetEditor.Gui.UserControls.AssetRowControls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Gui.UserControls.AssetTabControls
{
	public partial class ShadersAssetTabControl : UserControl
	{
		public static readonly DependencyProperty BinaryFileTypeProperty = DependencyProperty.Register
		(
			nameof(BinaryFileType),
			typeof(string),
			typeof(ShadersAssetTabControl)
		);

		public string BinaryFileType
		{
			get => (string)GetValue(BinaryFileTypeProperty);
			set => SetValue(BinaryFileTypeProperty, value);
		}

		internal ShadersAssetTabControlHandler Handler { get; private set; }

		public ShadersAssetTabControl()
		{
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= UserControl_Loaded;

			Handler = new ShadersAssetTabControlHandler((BinaryFileType)Enum.Parse(typeof(BinaryFileType), BinaryFileType, true));

			foreach (ShaderAssetRowControl arc in Handler.CreateAssetRowControls())
				AssetEditor.Items.Add(arc);
		}

		private void AssetEditor_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ShaderAssetRowControl arc = e.AddedItems[0] as ShaderAssetRowControl;

			Handler.SelectAsset(arc.Handler.Asset);
			Previewer.Initialize(arc.Handler.Asset);
		}
	}

	internal class ShadersAssetTabControlHandler : AbstractAssetTabControlHandler<ShaderAsset, ShaderAssetRowControl>
	{
		private protected override string AssetTypeJsonFileName => "Shaders";

		internal ShadersAssetTabControlHandler(BinaryFileType binaryFileType)
			: base(binaryFileType)
		{
		}

		internal override void UpdateGui(ShaderAsset asset)
		{
			ShaderAssetRowControl arc = assetRowControls.Where(a => a.Handler.Asset == asset).FirstOrDefault();
			arc.TextBlockEditorPath.Text = asset.EditorPath;
		}

		internal override string FileNameToChunkName(string fileName) => fileName.Replace("_fragment", "").Replace("_vertex", "");
	}
}