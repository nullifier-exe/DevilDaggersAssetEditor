using DevilDaggersAssetCore.BinaryFileHandlers;
using DevilDaggersAssetCore.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevilDaggersAssetCore.Compression
{
	public static class Compressor
	{
		private static readonly byte ddIdentifier = 66;

		public static byte[] CompressDd(string inputPath)
		{
			byte[] sourceFileBytes = File.ReadAllBytes(inputPath);

			ResourceFileHandler handler = new ResourceFileHandler(BinaryFileType.Dd);
			handler.ValidateFile(sourceFileBytes);
			byte[] tocBuffer = handler.ReadTocBuffer(sourceFileBytes);
			List<AbstractChunk> chunks = handler.ReadChunks(tocBuffer).Where(c => c.Size != 0).ToList(); // Filter empty chunks (garbage in core file TOC buffer).
			foreach (AbstractResourceChunk chunk in chunks)
			{
				byte[] buf = new byte[chunk.Size];
				Buffer.BlockCopy(sourceFileBytes, (int)chunk.StartOffset, buf, 0, (int)chunk.Size);
				chunk.SetBuffer(buf);
			}

			List<CompressedChunk> compressedChunks = new List<CompressedChunk>();
			foreach (AbstractChunk chunk in chunks)
			{
				CompressedChunk compressedChunk = new CompressedChunk(chunk);
				compressedChunks.Add(compressedChunk);
			}

			using MemoryStream tocStream = new MemoryStream();
			using MemoryStream dataStream = new MemoryStream();
			foreach (CompressedChunk compressedChunk in compressedChunks)
			{
				tocStream.Write(Encoding.Default.GetBytes(compressedChunk.Name), 0, compressedChunk.Name.Length);
				tocStream.Write(new[] { compressedChunk.Type }, 0, sizeof(byte));
				tocStream.Write(BitConverter.GetBytes(compressedChunk.Buffer.Length), 0, sizeof(uint));

				dataStream.Write(compressedChunk.Buffer, 0, compressedChunk.Buffer.Length);
			}

			byte[] header = new byte[3];
			Buffer.BlockCopy(new byte[1] { ddIdentifier }, 0, header, 0, 1);
			Buffer.BlockCopy(BitConverter.GetBytes((ushort)tocStream.Length), 0, header, 1, sizeof(ushort));

			using MemoryStream finalStream = new MemoryStream();
			finalStream.Write(header, 0, header.Length);
			finalStream.Write(tocStream.ToArray(), 0, (int)tocStream.Length);
			finalStream.Write(dataStream.ToArray(), 0, (int)dataStream.Length);

			return finalStream.ToArray();
		}
	}
}