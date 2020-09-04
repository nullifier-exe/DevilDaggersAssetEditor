﻿using DevilDaggersAssetEditor.Assets;
using DevilDaggersAssetEditor.Wpf.Gui.UserControls.AssetRowControls;
using System.IO;

namespace DevilDaggersAssetEditor.Wpf.Code.RowControlHandlers
{
	public class TextureAssetRowControlHandler : AbstractAssetRowControlHandler<TextureAsset, TextureAssetRowControl>
	{
		public TextureAssetRowControlHandler(TextureAsset asset, bool isEven)
			: base(asset, isEven)
		{
		}

		public override string OpenDialogFilter => "Texture files (*.png)|*.png";

		public override void UpdateGui()
		{
			AssetRowControl.TextBlockDescription.Text = Asset.Description.TrimRight(EditorUtils.DescriptionMaxLength);
			AssetRowControl.TextBlockEditorPath.Text = File.Exists(Asset.EditorPath) ? Asset.EditorPath.TrimLeft(EditorUtils.EditorPathMaxLength) : Utils.FileNotFound;
		}
	}
}