using DevilDaggersAssetCore;
using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetCore.BinaryFileHandlers;
using DevilDaggersAssetCore.Compression;
using DevilDaggersAssetCore.ModFiles;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace DevilDaggersAssetEditor.Code.FileTabControlHandlers
{
	public class DdFileTabControlHandler : AbstractFileTabControlHandler
	{
		public override AbstractBinaryFileHandler FileHandler => new ResourceFileHandler(BinaryFileType.Dd);

		public override MenuItem CreateFileTypeMenuItem()
		{
			MenuItem fileTypeMenuItem = base.CreateFileTypeMenuItem();

			MenuItem modelBindingImport = new MenuItem { Header = "Import Model Binding paths from folder" };
			MenuItem modelImport = new MenuItem { Header = "Import Model paths from folder" };
			MenuItem shaderImport = new MenuItem { Header = "Import Shader paths from folder" };
			MenuItem textureImport = new MenuItem { Header = "Import Texture paths from folder" };

			modelBindingImport.Click += (sender, e) => App.Instance.MainWindow.DdModelBindingsAssetTabControl.Handler.ImportFolder();
			modelImport.Click += (sender, e) => App.Instance.MainWindow.DdModelsAssetTabControl.Handler.ImportFolder();
			shaderImport.Click += (sender, e) => App.Instance.MainWindow.DdShadersAssetTabControl.Handler.ImportFolder();
			textureImport.Click += (sender, e) => App.Instance.MainWindow.DdTexturesAssetTabControl.Handler.ImportFolder();

			fileTypeMenuItem.Items.Add(modelBindingImport);
			fileTypeMenuItem.Items.Add(modelImport);
			fileTypeMenuItem.Items.Add(shaderImport);
			fileTypeMenuItem.Items.Add(textureImport);

			fileTypeMenuItem.Items.Add(new Separator());

			MenuItem compressDd = new MenuItem { Header = "Compress DD" };
			compressDd.Click += (sender, e) =>
			{
				OpenFileDialog openDialog = new OpenFileDialog();
				bool? openResult = openDialog.ShowDialog();
				if (openResult.HasValue && openResult.Value)
				{
					byte[] compressedBytes = Compressor.Compress(openDialog.FileName);
					File.WriteAllBytes($"{openDialog.FileName}.cdd", compressedBytes);
				}
			};
			fileTypeMenuItem.Items.Add(compressDd);

			MenuItem extractCdd = new MenuItem { Header = "Extract cdd" };
			extractCdd.Click += (sender, e) =>
			{
				OpenFileDialog openDialog = new OpenFileDialog { Filter = "Compressed DD files (*.cdd)|*.cdd" };
				bool? openResult = openDialog.ShowDialog();
				if (openResult.HasValue && openResult.Value)
				{
					byte[] extractedBytes = Compressor.Extract(openDialog.FileName);
					File.WriteAllBytes($"{openDialog.FileName}-extracted-from-cdd", extractedBytes);
				}
			};
			fileTypeMenuItem.Items.Add(extractCdd);

			return fileTypeMenuItem;
		}

		public override List<AbstractAsset> GetAssets()
			=> App.Instance.MainWindow.DdModelBindingsAssetTabControl.Handler.RowHandlers.Select(a => a.Asset).Cast<AbstractAsset>()
				.Concat(App.Instance.MainWindow.DdModelsAssetTabControl.Handler.RowHandlers.Select(a => a.Asset).Cast<AbstractAsset>())
				.Concat(App.Instance.MainWindow.DdShadersAssetTabControl.Handler.RowHandlers.Select(a => a.Asset).Cast<AbstractAsset>())
				.Concat(App.Instance.MainWindow.DdTexturesAssetTabControl.Handler.RowHandlers.Select(a => a.Asset).Cast<AbstractAsset>())
				.ToList();

		public override void UpdateAssetTabControls(List<AbstractUserAsset> assets)
		{
			UpdateAssetTabControl(assets.OfType<ModelBindingUserAsset>().ToList(), App.Instance.MainWindow.DdModelBindingsAssetTabControl.Handler);
			UpdateAssetTabControl(assets.OfType<ModelUserAsset>().ToList(), App.Instance.MainWindow.DdModelsAssetTabControl.Handler);
			UpdateAssetTabControl(assets.OfType<ShaderUserAsset>().ToList(), App.Instance.MainWindow.DdShadersAssetTabControl.Handler);
			UpdateAssetTabControl(assets.OfType<TextureUserAsset>().ToList(), App.Instance.MainWindow.DdTexturesAssetTabControl.Handler);
		}

		protected override bool IsComplete()
			=> App.Instance.MainWindow.DdModelBindingsAssetTabControl.Handler.IsComplete()
			&& App.Instance.MainWindow.DdModelsAssetTabControl.Handler.IsComplete()
			&& App.Instance.MainWindow.DdShadersAssetTabControl.Handler.IsComplete()
			&& App.Instance.MainWindow.DdTexturesAssetTabControl.Handler.IsComplete();
	}
}