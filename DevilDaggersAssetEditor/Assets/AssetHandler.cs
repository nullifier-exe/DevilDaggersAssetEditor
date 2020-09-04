﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DevilDaggersAssetEditor.Assets
{
	public sealed class AssetHandler
	{
		private static readonly Lazy<AssetHandler> _lazy = new Lazy<AssetHandler>(() => new AssetHandler());

		private AssetHandler()
		{
			using StreamReader srAudioAudio = new StreamReader(Utils.GetContentStream("audio.Audio.json"));
			AudioAudioAssets = JsonConvert.DeserializeObject<List<AudioAsset>>(srAudioAudio.ReadToEnd());

			using StreamReader srCoreShaders = new StreamReader(Utils.GetContentStream("core.Shaders.json"));
			CoreShadersAssets = JsonConvert.DeserializeObject<List<ShaderAsset>>(srCoreShaders.ReadToEnd());

			using StreamReader srDdModelBindings = new StreamReader(Utils.GetContentStream("dd.Model Bindings.json"));
			DdModelBindingsAssets = JsonConvert.DeserializeObject<List<ModelBindingAsset>>(srDdModelBindings.ReadToEnd());

			using StreamReader srDdModels = new StreamReader(Utils.GetContentStream("dd.Models.json"));
			DdModelsAssets = JsonConvert.DeserializeObject<List<ModelAsset>>(srDdModels.ReadToEnd());

			using StreamReader srDdShaders = new StreamReader(Utils.GetContentStream("dd.Shaders.json"));
			DdShadersAssets = JsonConvert.DeserializeObject<List<ShaderAsset>>(srDdShaders.ReadToEnd());

			using StreamReader srDdTextures = new StreamReader(Utils.GetContentStream("dd.Textures.json"));
			DdTexturesAssets = JsonConvert.DeserializeObject<List<TextureAsset>>(srDdTextures.ReadToEnd());

			using StreamReader srParticleParticles = new StreamReader(Utils.GetContentStream("particle.Particles.json"));
			ParticleParticlesAssets = JsonConvert.DeserializeObject<List<ParticleAsset>>(srParticleParticles.ReadToEnd());
		}

		public static AssetHandler Instance => _lazy.Value;

		public List<AudioAsset> AudioAudioAssets { get; }
		public List<ShaderAsset> CoreShadersAssets { get; }
		public List<ModelBindingAsset> DdModelBindingsAssets { get; }
		public List<ModelAsset> DdModelsAssets { get; }
		public List<ShaderAsset> DdShadersAssets { get; }
		public List<TextureAsset> DdTexturesAssets { get; }
		public List<ParticleAsset> ParticleParticlesAssets { get; }

		public List<AbstractAsset> GetAssets(BinaryFileType binaryFileType, string assetType)
		{
			string id = $"{binaryFileType.ToString().ToLower(CultureInfo.InvariantCulture)}.{assetType.ToLower(CultureInfo.InvariantCulture)}";

			return id switch
			{
				"audio.audio" => AudioAudioAssets.Cast<AbstractAsset>().ToList(),
				"core.shaders" => CoreShadersAssets.Cast<AbstractAsset>().ToList(),
				"dd.model bindings" => DdModelBindingsAssets.Cast<AbstractAsset>().ToList(),
				"dd.models" => DdModelsAssets.Cast<AbstractAsset>().ToList(),
				"dd.shaders" => DdShadersAssets.Cast<AbstractAsset>().ToList(),
				"dd.textures" => DdTexturesAssets.Cast<AbstractAsset>().ToList(),
				"particle.particles" => ParticleParticlesAssets.Cast<AbstractAsset>().ToList(),
				_ => throw new Exception($"No asset data found for binary file type '{binaryFileType}' and asset type '{assetType}.'")
			};
		}
	}
}