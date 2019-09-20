﻿using DevilDaggersAssetCore;
using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetEditor.GUI.UserControls.AssetControls;
using System.Linq;

namespace DevilDaggersAssetEditor.Code.ExpanderControlHandlers
{
	public class ShadersExpanderControlHandler : AbstractExpanderControlHandler<ShaderAsset, ShaderAssetControl>
	{
		protected override string AssetTypeJsonFileName => "Shaders";

		public ShadersExpanderControlHandler(BinaryFileType binaryFileType)
			: base(binaryFileType)
		{
		}

		public override void UpdateGUI(ShaderAsset asset)
		{
			ShaderAssetControl ac = assetControls.Where(a => a.Handler.Asset == asset).FirstOrDefault();
			ac.TextBlockEditorPath.Text = asset.EditorPath;
		}
	}
}