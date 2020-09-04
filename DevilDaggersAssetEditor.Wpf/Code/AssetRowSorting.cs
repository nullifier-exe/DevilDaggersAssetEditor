﻿using DevilDaggersAssetEditor.Assets;
using DevilDaggersAssetEditor.Wpf.Code.RowControlHandlers;
using System;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Wpf.Code
{
	public class AssetRowSorting<TAsset, TAssetRowControl, TAssetRowControlHandler>
		where TAsset : AbstractAsset
		where TAssetRowControl : UserControl
		where TAssetRowControlHandler : AbstractAssetRowControlHandler<TAsset, TAssetRowControl>
	{
		public AssetRowSorting(Func<TAssetRowControlHandler, object> sortingFunction)
		{
			SortingFunction = sortingFunction;
		}

		public Func<TAssetRowControlHandler, object> SortingFunction { get; set; }
		public bool IsAscending { get; set; } = true;
	}
}