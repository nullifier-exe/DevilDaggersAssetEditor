﻿using DevilDaggersAssetCore;
using System.Windows;
using System.Windows.Controls;
using DevilDaggersAssetEditor.GUI.UserControls.AssetControls;
using System;
using DevilDaggersAssetEditor.Code.AssetTabControlHandlers;
using DevilDaggersAssetCore.Assets;
using System.Windows.Controls.Primitives;
using IrrKlang;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace DevilDaggersAssetEditor.GUI.UserControls.AssetTabControls
{
	public partial class AudioAssetTabControl : UserControl
	{
		private readonly ISoundEngine engine = new ISoundEngine();

		public ISound Song { get; private set; }
		public ISoundSource SongData { get; private set; }

		private bool dragging;

		public static readonly DependencyProperty BinaryFileTypeProperty = DependencyProperty.Register
		(
			nameof(BinaryFileType),
			typeof(string),
			typeof(AudioAssetTabControl)
		);

		public string BinaryFileType
		{
			get => (string)GetValue(BinaryFileTypeProperty);
			set => SetValue(BinaryFileTypeProperty, value);
		}

		public AudioAssetTabControlHandler Handler { get; private set; }

		public AudioAssetTabControl()
		{
			InitializeComponent();
			ToggleImage.Source = ((Image)Resources["PlayImage"]).Source;

			DispatcherTimer timer = new DispatcherTimer
			{
				Interval = new TimeSpan(0, 0, 0, 0, 10)
			};
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (Song != null && !Song.Paused)
			{
				if (!dragging)
					Seek.Value = Song.PlayPosition / (float)Song.PlayLength * Seek.Maximum;

				SeekText.Text = $"{ToTimeString((int)Song.PlayPosition)} / {ToTimeString((int)Song.PlayLength)}";
			}
		}

		private string ToTimeString(int milliseconds)
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, milliseconds);
			if (timeSpan.Days > 0)
				return $"{timeSpan:dd\\:hh\\:mm\\:ss\\.fff}";
			if (timeSpan.Hours > 0)
				return $"{timeSpan:hh\\:mm\\:ss\\.fff}";
			return $"{timeSpan:mm\\:ss\\.fff}";
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= UserControl_Loaded;

			Handler = new AudioAssetTabControlHandler((BinaryFileType)Enum.Parse(typeof(BinaryFileType), BinaryFileType));

			foreach (AudioAssetControl ac in Handler.CreateUserControls())
			{
				AssetEditor.Children.Add(ac);
				ac.MouseDoubleClick += (senderAC, eAC) => Ac_MouseDoubleClick(ac.Handler.Asset);
			}
		}

		private void Ac_MouseDoubleClick(AudioAsset audioAsset)
		{
			AudioName.Text = audioAsset.AssetName;

			if (Song != null)
				Song.Stop();

			SongData = engine.GetSoundSource(audioAsset.EditorPath);
			Song = engine.Play2D(SongData, true, false, true);

			if (Song != null)
			{
				Seek.Maximum = Song.PlayLength;
				Song.PlaybackSpeed = (float)Pitch.Value;
				ToggleImage.Source = ((Image)Resources["PauseImage"]).Source;

				SeekText.Text = $"{ToTimeString((int)Song.PlayPosition)} / {ToTimeString((int)Song.PlayLength)}";
				PitchText.Text = $"x {Song.PlaybackSpeed:0.00}";
			}
		}

		private void Toggle_Click(object sender, RoutedEventArgs e)
		{
			if (Song != null)
				Song.Paused = !Song.Paused;

			if (Song.Paused)
				ToggleImage.Source = ((Image)Resources["PlayImage"]).Source;
			else
				ToggleImage.Source = ((Image)Resources["PauseImage"]).Source;
		}

		private void Pitch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (Song != null)
			{
				Song.PlaybackSpeed = (float)e.NewValue;
				PitchText.Text = $"x {Song.PlaybackSpeed:0.00}";
			}
		}

		private void Seek_DragStarted(object sender, DragStartedEventArgs e)
		{
			dragging = true;
		}

		private void Seek_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			dragging = false;

			if (Song != null)
				Song.PlayPosition = (uint)Seek.Value;
		}
	}
}