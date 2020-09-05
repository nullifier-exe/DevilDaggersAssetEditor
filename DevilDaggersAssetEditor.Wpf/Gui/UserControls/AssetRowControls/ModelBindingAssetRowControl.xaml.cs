﻿using DevilDaggersAssetEditor.Wpf.RowControlHandlers;
using System.Windows;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Wpf.Gui.UserControls.AssetRowControls
{
	public partial class ModelBindingAssetRowControl : UserControl
	{
		public ModelBindingAssetRowControl(ModelBindingAssetRowControlHandler handler)
		{
			Handler = handler;

			InitializeComponent();

			Data.Children.Add(Handler.TextBlockTags);
			Data.Children.Add(Handler.RectangleInfo);
			Data.Children.Add(Handler.RectangleEdit);

			Data.DataContext = Handler.Asset;
		}

		public ModelBindingAssetRowControlHandler Handler { get; }

		private void ButtonRemovePath_Click(object sender, RoutedEventArgs e)
			=> Handler.RemovePath();

		private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e)
			=> Handler.BrowsePath();

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
			=> Handler.UpdateGui();
	}
}