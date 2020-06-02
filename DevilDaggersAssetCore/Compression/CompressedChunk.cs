using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetCore.Chunks;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DevilDaggersAssetCore.Compression
{
	public class CompressedChunk
	{
		public byte Type { get; }
		public string Name { get; }
		public uint Size { get; }
		public byte[] Buffer { get; private set; }

		public CompressedChunk(byte type, string name, uint size)
		{
			Type = type;
			Name = name;
			Size = size;
		}

		public void Compress(AbstractChunk chunk)
		{
			if (chunk is TextureChunk textureChunk)
			{
				CompressTexture(textureChunk);
			}
			else
			{
				Buffer = chunk.Buffer;
			}
			//else if (chunk is ModelChunk modelChunk)
			//{

			//}
			//else if (chunk is ModelBindingChunk modelBindingChunk)
			//{

			//}
			//else if (chunk is ShaderChunk shaderChunk)
			//{

			//}
			//else if (chunk is AudioChunk audioChunk)
			//{

			//}
			//else if (chunk is ParticleChunk particleChunk)
			//{

			//}
			//else throw not implemented
		}

		public byte[] Extract()
		{
			return Type switch
			{
				0x02 => ExtractTexture(),
				_ => Buffer
			};
		}

		private void CompressTexture(TextureChunk textureChunk)
		{
			TextureAsset asset = AssetHandler.Instance.DdTexturesAssets.FirstOrDefault(t => t.AssetName == textureChunk.Name);
			bool usesTransparency = asset.IsModelTexture;
			PixelFormat pixelFormat = usesTransparency ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;

			IntPtr intPtr = Marshal.UnsafeAddrOfPinnedArrayElement(textureChunk.Buffer, 0);
			using Bitmap bitmap = new Bitmap((int)textureChunk.Header.Width, (int)textureChunk.Header.Height, (int)textureChunk.Header.Width * 4, pixelFormat, intPtr);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					Color pixel = bitmap.GetPixel(x, y);
					bitmap.SetPixel(x, y, Color.FromArgb(usesTransparency ? pixel.A : 255, pixel.B, pixel.G, pixel.R)); // Switch Blue and Red channels (reverse rgb).
				}
			}

			using MemoryStream memoryStream = new MemoryStream();
			// Create a new BitMap object to prevent "a generic GDI+ error" from being thrown.
			new Bitmap(bitmap).Save(memoryStream, ImageFormat.Png);

			Buffer = memoryStream.ToArray();
		}

		private byte[] ExtractTexture()
		{

		}
	}
}