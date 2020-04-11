﻿using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetEditor.Code;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevilDaggersAssetEditor.Gui.UserControls.AssetRowControls
{
	public partial class AudioAssetRowControl : UserControl
	{
		internal AudioAssetRowControlHandler Handler { get; private set; }

		public AudioAssetRowControl(AudioAsset asset)
		{
			InitializeComponent();

			Handler = new AudioAssetRowControlHandler(asset, this);

			Data.DataContext = asset;
		}

		private void ButtonRemovePath_Click(object sender, RoutedEventArgs e) => Handler.RemovePath();

		private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e) => Handler.BrowsePath();

		private bool ValidateTextBox(TextBox textBox)
		{
			bool valid = float.TryParse(textBox.Text, out _);

			textBox.Background = valid ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush(Color.FromRgb(255, 128, 128));

			return valid;
		}

		private void TextBoxLoudness_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (ValidateTextBox(TextBoxLoudness))
				Handler.Asset.Loudness = float.Parse(TextBoxLoudness.Text);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) => TextBoxLoudness.TextChanged += TextBoxLoudness_TextChanged;
	}

	internal class AudioAssetRowControlHandler : AbstractAssetRowControlHandler<AudioAsset, AudioAssetRowControl>
	{
		internal AudioAssetRowControlHandler(AudioAsset asset, AudioAssetRowControl parent)
			: base(asset, parent, "Audio files (*.wav)|*.wav")
		{
		}

		internal override void UpdateGui()
		{
			parent.TextBlockEditorPath.Text = Asset.EditorPath;
			parent.TextBoxLoudness.Text = Asset.Loudness.ToString();
		}
	}
}