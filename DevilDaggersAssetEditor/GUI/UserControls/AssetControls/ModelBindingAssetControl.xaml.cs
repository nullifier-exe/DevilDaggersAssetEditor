﻿using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetEditor.Code.AssetControlHandlers;
using System.Windows;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.GUI.UserControls.AssetControls
{
	public partial class ModelBindingAssetControl : UserControl
	{
		public ModelBindingAssetControlHandler Handler { get; private set; }

		public ModelBindingAssetControl(ModelBindingAsset asset)
		{
			InitializeComponent();

			Handler = new ModelBindingAssetControlHandler(asset, this);

			Data.DataContext = asset;
		}

		private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e)
		{
			Handler.BrowsePath();
		}
	}
}