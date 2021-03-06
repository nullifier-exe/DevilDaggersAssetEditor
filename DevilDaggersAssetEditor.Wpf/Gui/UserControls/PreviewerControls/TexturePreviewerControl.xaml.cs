﻿using DevilDaggersAssetEditor.Assets;
using DevilDaggersAssetEditor.Utils;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SdImage = System.Drawing.Image;

namespace DevilDaggersAssetEditor.Wpf.Gui.UserControls.PreviewerControls
{
	public partial class TexturePreviewerControl : UserControl, IPreviewerControl
	{
		public TexturePreviewerControl()
		{
			InitializeComponent();
		}

		public void Initialize(AbstractAsset asset)
		{
			if (asset is not TextureAsset textureAsset)
				return;

			TextureName.Content = textureAsset.AssetName;
			DefaultDimensions.Content = $"{textureAsset.DefaultDimensions.X}x{textureAsset.DefaultDimensions.Y}";
			DefaultMipmaps.Content = TextureAsset.GetMipmapCount(textureAsset.DefaultDimensions.X, textureAsset.DefaultDimensions.Y).ToString(CultureInfo.InvariantCulture);

			bool isPathValid = File.Exists(textureAsset.EditorPath);

			FileName.Content = isPathValid ? Path.GetFileName(textureAsset.EditorPath) : GuiUtils.FileNotFound;

			if (isPathValid)
			{
				using (SdImage image = SdImage.FromFile(textureAsset.EditorPath))
				{
					FileDimensions.Content = $"{image.Width}x{image.Height}";
					FileMipmaps.Content = TextureAsset.GetMipmapCount(image.Width, image.Height).ToString(CultureInfo.InvariantCulture);
				}

				using FileStream imageFileStream = new FileStream(textureAsset.EditorPath, FileMode.Open, FileAccess.Read);
				MemoryStream imageCopyStream = new MemoryStream();
				imageFileStream.CopyTo(imageCopyStream);
				BitmapImage src = new BitmapImage();
				src.BeginInit();
				src.StreamSource = imageCopyStream;
				src.EndInit();
				PreviewImage.Source = src;
			}
			else
			{
				PreviewImage.Source = null;
				FileDimensions.Content = "N/A";
				FileMipmaps.Content = "N/A";
			}
		}
	}
}
