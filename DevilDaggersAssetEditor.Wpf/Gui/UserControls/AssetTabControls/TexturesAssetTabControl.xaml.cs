﻿using DevilDaggersAssetEditor.Assets;
using DevilDaggersAssetEditor.Wpf.Code;
using DevilDaggersAssetEditor.Wpf.Code.RowControlHandlers;
using DevilDaggersAssetEditor.Wpf.Code.TabControlHandlers;
using DevilDaggersAssetEditor.Wpf.Gui.UserControls.AssetRowControls;
using DevilDaggersCore.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Wpf.Gui.UserControls.AssetTabControls
{
	public partial class TexturesAssetTabControl : UserControl
	{
		public static readonly DependencyProperty BinaryFileTypeProperty = DependencyProperty.Register(nameof(BinaryFileType), typeof(string), typeof(TexturesAssetTabControl));

		private readonly AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler> _nameSort = new AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler>((a) => a.Asset.AssetName);
		private readonly AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler> _tagsSort = new AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler>((a) => string.Join(", ", a.Asset.Tags));
		private readonly AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler> _descriptionSort = new AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler>((a) => a.Asset.Description);
		private readonly AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler> _pathSort = new AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler>((a) => a.Asset.EditorPath);

		public TexturesAssetTabControl()
		{
			InitializeComponent();
		}

		public string BinaryFileType
		{
			get => (string)GetValue(BinaryFileTypeProperty);
			set => SetValue(BinaryFileTypeProperty, value);
		}

		public TexturesAssetTabControlHandler Handler { get; private set; }

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= UserControl_Loaded;

			Handler = new TexturesAssetTabControlHandler((BinaryFileType)Enum.Parse(typeof(BinaryFileType), BinaryFileType, true));

			foreach (TextureAssetRowControl arc in Handler.RowHandlers.Select(a => a.AssetRowControl))
				AssetEditor.Items.Add(arc);

			CreateFiltersGui();
		}

		private void CreateFiltersGui()
		{
			FilterOperationAnd.Checked += ApplyFilter;
			FilterOperationOr.Checked += ApplyFilter;

			int columns = 9;
			int rows = (int)Math.Ceiling(Handler.FiltersCount / (double)columns);
			Handler.CreateFiltersGui(rows);

			for (int i = 0; i < columns; i++)
				Filters.ColumnDefinitions.Add(new ColumnDefinition());
			for (int i = 0; i < rows; i++)
				Filters.RowDefinitions.Add(new RowDefinition());

			foreach (CheckBox checkBox in Handler.FilterCheckBoxes)
			{
				checkBox.Checked += ApplyFilter;
				checkBox.Unchecked += ApplyFilter;
				Filters.Children.Add(checkBox);
			}
		}

		private void ApplyFilter(object sender, RoutedEventArgs e)
		{
			Handler.ApplyFilter(GetFilterOperation());

			foreach (TextureAssetRowControlHandler rowHandler in Handler.RowHandlers)
			{
				if (!rowHandler.IsActive)
				{
					if (AssetEditor.Items.Contains(rowHandler.AssetRowControl))
						AssetEditor.Items.Remove(rowHandler.AssetRowControl);
				}
				else
				{
					if (!AssetEditor.Items.Contains(rowHandler.AssetRowControl))
						AssetEditor.Items.Add(rowHandler.AssetRowControl);
				}
			}

			ApplySort();
		}

		private void ApplySort()
		{
			List<TextureAssetRowControlHandler> sorted = Handler.ApplySort();
			for (int i = 0; i < sorted.Count; i++)
			{
				TextureAssetRowControl arc = AssetEditor.Items.OfType<TextureAssetRowControl>().FirstOrDefault(arc => arc.Handler.Asset == sorted[i].Asset);
				AssetEditor.Items.Remove(arc);
				AssetEditor.Items.Insert(i, arc);
			}

			Handler.SetAssetEditorBackgroundColors(AssetEditor.Items);
		}

		private FilterOperation GetFilterOperation()
		{
			if (FilterOperationAnd.IsChecked())
				return FilterOperation.And;
			if (FilterOperationOr.IsChecked())
				return FilterOperation.Or;
			return FilterOperation.None;
		}

		private void AssetEditor_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				return;

			TextureAssetRowControl arc = e.AddedItems[0] as TextureAssetRowControl;

			Handler.SelectAsset(arc.Handler.Asset);
			Previewer.Initialize(arc.Handler.Asset);
		}

		private void NameSortButton_Click(object sender, RoutedEventArgs e)
			=> SetSorting(_nameSort);

		private void TagsSortButton_Click(object sender, RoutedEventArgs e)
			=> SetSorting(_tagsSort);

		private void DescriptionSortButton_Click(object sender, RoutedEventArgs e)
			=> SetSorting(_descriptionSort);

		private void PathSortButton_Click(object sender, RoutedEventArgs e)
			=> SetSorting(_pathSort);

		private void SetSorting(AssetRowSorting<TextureAsset, TextureAssetRowControl, TextureAssetRowControlHandler> sorting)
		{
			sorting.IsAscending = !sorting.IsAscending;
			Handler.ActiveSorting = sorting;

			ApplySort();
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
			=> Handler?.UpdateTagHighlighting();
	}
}