﻿using DevilDaggersAssetEditor.Assets;
using DevilDaggersAssetEditor.Utils;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Wpf.Gui.UserControls.PreviewerControls
{
	public partial class ModelPreviewerControl : UserControl, IPreviewerControl
	{
		public ModelPreviewerControl()
		{
			InitializeComponent();
		}

		public void Initialize(AbstractAsset asset)
		{
			if (asset is not ModelAsset modelAsset)
				return;

			TextureName.Content = modelAsset.AssetName;
			DefaultVertexCount.Content = modelAsset.DefaultVertexCount.ToString(CultureInfo.InvariantCulture);
			DefaultIndexCount.Content = modelAsset.DefaultIndexCount.ToString(CultureInfo.InvariantCulture);

			bool isPathValid = File.Exists(modelAsset.EditorPath);

			FileName.Content = isPathValid ? Path.GetFileName(modelAsset.EditorPath) : GuiUtils.FileNotFound;

			if (isPathValid)
			{
				string[] lines = File.ReadAllLines(modelAsset.EditorPath);
				int v = 0;
				int vt = 0;
				int vn = 0;
				int f = 0;
				foreach (string line in lines)
				{
					switch (line.Split(' ')[0])
					{
						case "v": v++; break;
						case "vt": vt++; break;
						case "vn": vn++; break;
						case "f": f++; break;
					}
				}

				FileVertexCount.Content = new[] { v, vt, vn }.Max().ToString(CultureInfo.InvariantCulture);
				FileIndexCount.Content = f.ToString(CultureInfo.InvariantCulture);

				// TODO: Open in OBJ Viewer
			}
			else
			{
				FileVertexCount.Content = "N/A";
				FileIndexCount.Content = "N/A";
			}
		}
	}
}
