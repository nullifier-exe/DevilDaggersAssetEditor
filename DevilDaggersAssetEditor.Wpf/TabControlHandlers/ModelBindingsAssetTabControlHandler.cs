﻿using DevilDaggersAssetEditor.Assets;
using DevilDaggersAssetEditor.BinaryFileHandlers;
using DevilDaggersAssetEditor.Wpf.Gui.UserControls.AssetRowControls;
using DevilDaggersAssetEditor.Wpf.RowControlHandlers;

namespace DevilDaggersAssetEditor.Wpf.TabControlHandlers
{
	public class ModelBindingsAssetTabControlHandler : AbstractAssetTabControlHandler<ModelBindingAsset, ModelBindingAssetRowControl, ModelBindingAssetRowControlHandler>
	{
		public ModelBindingsAssetTabControlHandler(BinaryFileType binaryFileType)
			: base(binaryFileType)
		{
		}

		protected override string AssetTypeJsonFileName => "Model Bindings";
	}
}