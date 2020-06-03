using DevilDaggersAssetCore.Assets;
using DevilDaggersAssetCore.Chunks;
using DevilDaggersAssetCore.Headers;
using DevilDaggersAssetCore.Info;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Buf = System.Buffer;

namespace DevilDaggersAssetCore.Compression
{
	public class TextureCompressedChunk : CompressedChunk
	{
		public TextureCompressedChunk(byte type, string name, uint size)
			: base(type, name, size)
		{
		}

		public override void Compress(AbstractResourceChunk chunk)
		{
			// TODO: Use C# 9 "not" operator.
			if (!(chunk is TextureChunk textureChunk))
				throw new Exception($"{chunk} was not of type {nameof(TextureChunk)}");

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

		// TODO: Use C# 9 covariant returns.
		public override AbstractResourceChunk Extract()
		{
			Image image = Image.FromStream(new MemoryStream(Buffer));
			TextureChunk textureChunk = new TextureChunk(Name, 0U, 0U, 0U);
			textureChunk.MakeBinary(image);

			byte[] headerBuffer = new byte[ChunkInfo.Texture.HeaderInfo.FixedSize.Value];
			Buf.BlockCopy(BitConverter.GetBytes((ushort)16401), 0, headerBuffer, 0, sizeof(ushort));
			Buf.BlockCopy(BitConverter.GetBytes(image.Width), 0, headerBuffer, 2, sizeof(uint));
			Buf.BlockCopy(BitConverter.GetBytes(image.Height), 0, headerBuffer, 6, sizeof(uint));
			headerBuffer[10] = TextureChunk.GetMipmapCountFromImage(image);

			textureChunk.Header = new TextureHeader(headerBuffer);

			return textureChunk;
		}
	}
}