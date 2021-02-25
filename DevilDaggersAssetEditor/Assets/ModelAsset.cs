﻿using DevilDaggersAssetEditor.ModFiles;
using System.Collections.Generic;

namespace DevilDaggersAssetEditor.Assets
{
	public class ModelAsset : AbstractAsset
	{
		public ModelAsset(string assetName, string description, List<string> tags, bool isProhibited, int defaultVertexCount, int defaultIndexCount)
			: base(assetName, AssetType.Model, description, tags, isProhibited)
		{
			DefaultVertexCount = defaultVertexCount;
			DefaultIndexCount = defaultIndexCount;
		}

		public int DefaultVertexCount { get; }
		public int DefaultIndexCount { get; }

		public override UserAsset ToUserAsset()
			=> new(AssetType.Model, AssetName, EditorPath);
	}
}
